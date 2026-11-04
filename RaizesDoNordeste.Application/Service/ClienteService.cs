using RaizesNordeste.Api;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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

    public async Task<Cliente> CriarClienteAsync(CriarClienteDto dto)
    {
        if (await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf))
            throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Cpf = dto.Cpf,
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

        // UC03 <<include>> UC09: Registra log de auditoria da anonimização LGPD
        await _auditoriaService.RegistrarOcorrenciaAsync(
            idUnidade: null,
            tipoAcao: "CLIENTE_ANONIMIZADO_LGPD",
            idAfetado: cliente.Id.ToString(),
            ator: "TitularDadosLGPD",
            detalhes: "Dados pessoais (Nome, Email, CPF) anonimizados a pedido do titular."
        );
    }
}
