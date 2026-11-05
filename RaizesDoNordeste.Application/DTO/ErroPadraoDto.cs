using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record ErroPadraoDto(
        int Status,
        string Erro,
        string Mensagem,
        string Path,
        DateTime Timestamp
    );
}
