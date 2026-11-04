using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.Service;
public class AuditoriaService
{
    private readonly AppDbContext _context;

    public AuditoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarOcorrenciaAsync(Guid? idUnidade, string tipoAcao, string idAfetado, string ator, string detalhes)
    {
        var ocorrencia = Ocorrencia.RegistrarLog(idUnidade, tipoAcao, idAfetado, ator, detalhes);
        _context.Ocorrencias.Add(ocorrencia);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Ocorrencia>> ConsultarRelatorioAsync(Guid? idUnidade)
    {
        var query = _context.Ocorrencias.AsNoTracking().AsQueryable();

        if (idUnidade.HasValue)
            query = query.Where(o => o.IdUnidade == idUnidade.Value);

        return await query
            .OrderByDescending(o => o.DataOcorrencia)
            .ToListAsync();
    }
}
