using RaizesNordeste.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO {
    public record PaginacaoPedidosDto(
    int Page,
    int Limit,
    int TotalItens,
    int TotalPaginas,
    List<Pedido> Dados
);
}
