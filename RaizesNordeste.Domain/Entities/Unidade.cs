using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Unidade
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public bool CozinhaCompleta { get; set; }
        public bool Ativo { get; set; } = true;

        public List<Item> ItensCardapio { get; set; } = new();

        public List<Item> ObterCardapioAtivo()
        {
            return ItensCardapio.Where(i => i.ValidarDisponibilidade()).ToList();
        }
    }
}
