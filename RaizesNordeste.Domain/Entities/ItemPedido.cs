using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Entities
{
    public class ItemPedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdPedido { get; set; }
        public Guid IdItem { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public decimal CalcularSubtotal() => Quantidade * PrecoUnitario;
    }
}
