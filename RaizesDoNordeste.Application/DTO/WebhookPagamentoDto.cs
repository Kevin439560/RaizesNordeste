using RaizesNordeste.Domain.Enums;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record WebhookPagamentoDto(
        Guid IdPedido,
        bool Aprovado,
        string IdTransacao
    );
}
