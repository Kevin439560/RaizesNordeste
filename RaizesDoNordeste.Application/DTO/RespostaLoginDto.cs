using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record RespostaLoginDto(
        string AccessToken,
        string TipoToken,
        int ExpiraEmSegundos,
        Guid IdUsuario,
        string Nome,
        string Perfil
    );
}
