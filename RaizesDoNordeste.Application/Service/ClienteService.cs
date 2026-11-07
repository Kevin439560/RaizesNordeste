using Microsoft.EntityFrameworkCore;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Dominio.Enums;
using RaizesNordeste.Infra.Data;

namespace RaizesDoNordeste.Aplicacao.Service;
public class ClienteService
{
    private readonly AppDbContext _context;
    private readonly AuditoriaService _auditoriaService;

    public ClienteService(AppDbContext context, AuditoriaService auditoriaService)
    {
        _context = context;
        _auditoriaService = auditoriaService;
    }

    public async Task<Cliente> AutenticarAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            throw new ArgumentException("E-mail e senha são obrigatórios.");

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == dto.Email && !c.Anonimo);

        if (cliente == null || !cliente.ValidarSenha(dto.Senha))
            throw new UnauthorizedAccessException("Credenciais inválidas (e-mail ou senha incorretos).");

        return cliente;
    }

    public async Task<Cliente> CriarClienteAsync(CriarClienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Senha))
            throw new ArgumentException("Nome, Email, CPF e Senha são campos obrigatórios.");

        if (!dto.Email.Contains('@') || !dto.Email.Contains('.'))
            throw new ArgumentException("Formato de e-mail inválido.");

        if (await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf))
            throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Cpf = dto.Cpf,
            senhaHash = Cliente.GerarSenhaHash(dto.Senha),
            perfilUsuario = dto.Perfil ?? PerfilUsuario.CLIENTE,
            ConsenteTermosLgpd = dto.ConsenteTermosLgpd
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return cliente;
    }

    public async Task<int> ObterSaldoPontosAsync(Guid id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        if (cliente.Anonimo)
            throw new InvalidOperationException("Este cadastro foi anonimizado e não possui programa de pontos ativo.");

        return cliente.Pontos;
    }

    public async Task AnonimizarClienteAsync(Guid id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        if (cliente.Anonimo)
            throw new InvalidOperationException("Os dados deste cliente já foram anonimizados.");

        cliente.AnonimizarDados();
        await _context.SaveChangesAsync();

        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: null,
            tipoAcao: "CLIENTE_ANONIMIZADO_LGPD",
            idAfetado: cliente.Id.ToString(),
            ator: "TitularDadosLGPD",
            detalhes: "Dados pessoais (Nome, Email, CPF) anonimizados a pedido do titular."
        );
    }
}
