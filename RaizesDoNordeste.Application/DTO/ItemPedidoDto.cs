using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record ItemPedidoDto(
        Guid IdItem,
        int Quantidade
    );
}
