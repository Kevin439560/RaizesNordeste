using RaizesNordeste.Domain.Enums;

namespace RaizesNordeste.Api
{
    public record CriarClienteDto(
    string Nome,
    string Email,
    string Cpf,
    bool ConsenteTermosLgpd
);

    public record ItemPedidoDto(
        Guid IdItem,
        int Quantidade
    );

    public record CriarPedidoDto(
        Guid IdUnidade,
        Guid? IdCliente,
        CanalPedido Canal,
        List<ItemPedidoDto> Itens
    );

    public record WebhookPagamentoDto(
        Guid IdPedido,
        bool Aprovado,
        string IdTransacao
    );
}
