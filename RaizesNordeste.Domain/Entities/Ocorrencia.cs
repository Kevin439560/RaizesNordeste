using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Ocorrencia
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? IdUnidade { get; set; }
        public string TipoAcao { get; set; } = string.Empty;
        public string IdAfetado { get; set; } = string.Empty;
        public string Ator { get; set; } = string.Empty;
        public string Detalhes { get; set; } = string.Empty;
        public DateTime DataOcorrencia { get; set; } = DateTime.UtcNow;

        public static Ocorrencia RegistrarLog(Guid? idUnidade, string acao, string idAfetado, string ator, string detalhes)
        {
            return new Ocorrencia
            {
                IdUnidade = idUnidade,
                TipoAcao = acao,
                IdAfetado = idAfetado,
                Ator = ator,
                Detalhes = detalhes
            };
        }
    }
}
