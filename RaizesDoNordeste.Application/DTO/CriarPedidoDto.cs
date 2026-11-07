using RaizesNordeste.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record CriarPedidoDto(
        Guid IdUnidade,
        Guid? IdCliente,
        CanalPedido? Canal,
        List<ItemPedidoDto> Itens,
        string FormaPagamento = "MOCK",
        int? PontosParaResgatar = null
    );
}
