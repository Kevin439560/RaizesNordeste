using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Domain.Enums;
using RaizesNordeste.Infra.Data;


namespace RaizesDoNordeste.Aplicacao.Service;
public class PedidoService
{
    private readonly AppDbContext _context;
    private readonly AuditoriaService _auditoriaService;

    public PedidoService(AppDbContext context, AuditoriaService auditoriaService)
    {
        _context = context;
        _auditoriaService = auditoriaService;
    }

    public async Task<List<Unidade>> ListarUnidadesAsync()
    {
        return await _context.Unidades.AsNoTracking().Where(u => u.Ativo).ToListAsync();
    }

    public async Task<List<Item>> ObterCardapioUnidadeAsync(Guid idUnidade)
    {
        var unidadeExiste = await _context.Unidades.AnyAsync(u => u.Id == idUnidade && u.Ativo);
        if (!unidadeExiste)
            throw new KeyNotFoundException("Unidade não encontrada ou inativa.");

        var itens = await _context.Itens
            .AsNoTracking()
            .Where(i => i.IdUnidade == idUnidade && i.Disponivel)
            .ToListAsync();

        return [.. itens.Where(i => i.ValidarDisponibilidade())];
    }

    public async Task<Item> MovimentarEstoqueAsync(Guid idItem, MovimentarEstoqueDto dto, string ator)
    {
        if (dto.QuantidadeDiferenca == 0)
            throw new ArgumentException("A quantidade de itens movimentados não pode ser zero.");

        var item = await _context.Itens.FirstOrDefaultAsync(i => i.Id == idItem)
            ?? throw new KeyNotFoundException("Produto/Item não encontrado no estoque.");

        item.MovimentarEstoque(dto.QuantidadeDiferenca);
        await _context.SaveChangesAsync();

        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: item.IdUnidade,
            tipoAcao: dto.QuantidadeDiferenca > 0 ? "ESTOQUE_ENTRADA" : "ESTOQUE_SAIDA_MANUAL",
            idAfetado: item.Id.ToString(),
            ator: ator,
            detalhes: $"Movimentação: {dto.QuantidadeDiferenca}. Saldo atual: {item.QuantidadeEstoque}. Motivo: {dto.Motivo}"
        );

        return item;
    }

    public async Task<Pedido> CriarPedidoAsync(CriarPedidoDto dto)
    {

        if (dto.Canal == null || !Enum.IsDefined(typeof(CanalPedido), dto.Canal.Value))
            throw new ArgumentException("O campo 'canalPedido' não foi informado ou com valor diferente do esperado");

        if (dto.Itens == null || !dto.Itens.Any())
            throw new ArgumentException("O pedido deve conter pelo menos um item.");

        var unidadeExiste = await _context.Unidades.AnyAsync(u => u.Id == dto.IdUnidade && u.Ativo);
        if (!unidadeExiste)
            throw new KeyNotFoundException("Unidade informada não existe ou está inativa.");

        var pedido = new Pedido
        {
            IdUnidade = dto.IdUnidade,
            IdCliente = dto.IdCliente,
            Canal = dto.Canal.Value,
            FormaPagamento = string.IsNullOrWhiteSpace(dto.FormaPagamento) ? "MOCK" : dto.FormaPagamento
        };

        foreach (var itemDto in dto.Itens)
        {
            if (itemDto.Quantidade <= 0)
                throw new ArgumentException($"A quantidade do item {itemDto.IdItem} deve ser maior que zero.");

            var itemCardapio = await _context.Itens
                .FirstOrDefaultAsync(i => i.Id == itemDto.IdItem && i.IdUnidade == dto.IdUnidade)
                ?? throw new KeyNotFoundException($"O produto/item {itemDto.IdItem} não foi encontrado nesta unidade.");

            if (itemCardapio == null || !itemCardapio.ValidarDisponibilidade())
                throw new InvalidOperationException($"O item {itemDto.IdItem} não está disponível nesta unidade.");

            itemCardapio.BaixarEstoque(itemDto.Quantidade);

            pedido.Itens.Add(new ItemPedido
            {
                IdPedido = pedido.Id,
                IdItem = itemCardapio.Id,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = itemCardapio.Preco
            });
        }

        decimal descontoFidelidade = 0m;
        if (dto.PontosParaResgatar.HasValue && dto.PontosParaResgatar.Value > 0)
        {
            if (!dto.IdCliente.HasValue)
                throw new ArgumentException("Para resgatar pontos é necessário informar o cliente.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.IdCliente.Value)
                ?? throw new KeyNotFoundException("Cliente não encontrado para resgate de pontos.");

            descontoFidelidade = cliente.ResgatarPontos(dto.PontosParaResgatar.Value);
        }

        pedido.CalcularValorTotal(descontoFidelidade);

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: pedido.IdUnidade,
            tipoAcao: "PEDIDO_CRIADO",
            idAfetado: pedido.Id.ToString(),
            ator: pedido.IdCliente?.ToString() ?? $"Anonimo_{pedido.Canal}",
            detalhes: $"Pedido criado via canal {pedido.Canal}. Valor total: R$ {pedido.ValorTotal:F2}."
        );

        return pedido;
    }

    public async Task<object> ListarPedidosAsync(CanalPedido? canalPedido, StatusPedido? status, int page = 1, int limit = 10)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var query = _context.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .AsQueryable();

        if (canalPedido.HasValue)
            query = query.Where(p => p.Canal == canalPedido.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var totalItens = await query.CountAsync();
        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return new
        {
            page,
            limit,
            totalItens,
            totalPaginas = (int)Math.Ceiling(totalItens / (double)limit),
            dados = itens
        };
    }

    public async Task<Pedido> ObterPorIdAsync(Guid idPedido)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == idPedido)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");
    }

    public async Task ProcessarConfirmacaoPagamentoAsync(Guid idPedido, bool aprovado, string idTransacao)
    {

        if (string.IsNullOrWhiteSpace(idTransacao))
            throw new ArgumentException("O código da transação externa (idTransacao) é obrigatório.");

        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == idPedido)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (pedido.IdTransacaoExterna == idTransacao && pedido.Status != StatusPedido.AGUARDANDO_PAGAMENTO)
            return;

        if (aprovado)
        {
            pedido.ConfirmarPagamento(idTransacao);
            
            if (pedido.IdCliente.HasValue)
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == pedido.IdCliente.Value);
                cliente?.AdicionarPontos(pedido.ValorTotal);
            }
        }
        else
        {
            pedido.IdTransacaoExterna = idTransacao;
            pedido.AtualizarStatus(StatusPedido.PAGAMENTO_RECUSADO);

            foreach (var itemPedido in pedido.Itens)
            {
                var itemEstoque = await _context.Itens.FirstOrDefaultAsync(i => i.Id == itemPedido.IdItem);
                itemEstoque?.MovimentarEstoque(itemPedido.Quantidade);
            }

        }

        await _context.SaveChangesAsync();

        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: pedido.IdUnidade,
            tipoAcao: aprovado ? "PAGAMENTO_APROVADO" : "PAGAMENTO_RECUSADO",
            idAfetado: pedido.Id.ToString(),
            ator: "GatewayPagamento",
            detalhes: $"Webhook processado. Transação externa: {idTransacao}. Status do pedido: {pedido.Status}"
        );
    }

    public async Task AlterarStatusAsync(Guid idPedido, StatusPedido novoStatus, string? motivo)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == idPedido)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (novoStatus == StatusPedido.CANCELADO)
        {
            pedido.Cancelar(motivo ?? string.Empty);

            foreach (var itemPedido in pedido.Itens)
            {
                var itemEstoque = await _context.Itens.FirstOrDefaultAsync(i => i.Id == itemPedido.IdItem);
                itemEstoque?.MovimentarEstoque(itemPedido.Quantidade);
            }
        }
        else
        {
            pedido.AtualizarStatus(novoStatus);
        }

        await _context.SaveChangesAsync();
 
        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: pedido.IdUnidade,
            tipoAcao: $"STATUS_ALTERADO_{novoStatus}",
            idAfetado: pedido.Id.ToString(),
            ator: "OperadorUnidade",
            detalhes: novoStatus == StatusPedido.CANCELADO
                ? $"Pedido cancelado. Justificativa: {motivo}"
                : $"Status atualizado para {novoStatus}."
        );
    }
}