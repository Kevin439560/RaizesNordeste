using RaizesNordeste.Api;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Domain.Enums;
using RaizesNordeste.Infra.Data;
using System;
using System.Collections.Generic;
using System.Text;

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

    public async Task<List<Item>> ObterCardapioUnidadeAsync(Guid idUnidade)
    {
        var unidadeExiste = await _context.Unidades.AnyAsync(u => u.Id == idUnidade && u.Ativo);
        if (!unidadeExiste)
            throw new KeyNotFoundException("Unidade não encontrada ou inativa.");

        var itens = await _context.Itens
            .AsNoTracking()
            .Where(i => i.IdUnidade == idUnidade && i.Disponivel)
            .ToListAsync();

        return itens.Where(i => i.ValidarDisponibilidade()).ToList();
    }

    public async Task<Pedido> CriarPedidoAsync(CriarPedidoDto dto)
    {
        if (dto.Itens == null || !dto.Itens.Any())
            throw new ArgumentException("O pedido deve conter pelo menos um item.");

        var pedido = new Pedido
        {
            IdUnidade = dto.IdUnidade,
            IdCliente = dto.IdCliente,
            Canal = dto.Canal
        };

        foreach (var itemDto in dto.Itens)
        {
            var itemCardapio = await _context.Itens
                .FirstOrDefaultAsync(i => i.Id == itemDto.IdItem && i.IdUnidade == dto.IdUnidade);

            if (itemCardapio == null || !itemCardapio.ValidarDisponibilidade())
                throw new InvalidOperationException($"O item {itemDto.IdItem} não está disponível nesta unidade.");

            pedido.Itens.Add(new ItemPedido
            {
                IdPedido = pedido.Id,
                IdItem = itemCardapio.Id,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = itemCardapio.Preco
            });
        }

        pedido.CalcularValorTotal();

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return pedido;
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
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == idPedido)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        // Idempotência: se o webhook já processou essa transação antes, ignora
        if (pedido.IdTransacaoExterna == idTransacao && pedido.Status != StatusPedido.AGUARDANDO_PAGAMENTO)
            return;

        if (aprovado)
        {
            pedido.ConfirmarPagamento(idTransacao);

            // Se o pedido tem cliente vinculado, soma pontos de fidelidade (caso consinta LGPD)
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
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == idPedido)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");

        if (novoStatus == StatusPedido.CANCELADO)
        {
            pedido.Cancelar(motivo ?? string.Empty);
        }
        else
        {
            pedido.AtualizarStatus(novoStatus);
        }

        await _context.SaveChangesAsync();

        // UC08 <<include>> UC09: Registra mudança de status / cancelamento na auditoria
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