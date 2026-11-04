using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdUnidade { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public bool Disponivel { get; set; } = true;
        public bool ProdutoSazonal { get; set; } = false;

        public bool ValidarDisponibilidade()
        {
            return Disponivel;
        }
    }
}
