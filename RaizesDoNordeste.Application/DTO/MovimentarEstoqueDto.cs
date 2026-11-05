using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record MovimentarEstoqueDto(
        int QuantidadeDiferenca,
        string Motivo
    );
}
