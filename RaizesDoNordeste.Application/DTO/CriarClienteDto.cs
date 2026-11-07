using RaizesNordeste.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesDoNordeste.Aplicacao.DTO
{
    public record CriarClienteDto(
        string Nome,
        string Email,
        string Cpf,
        string Senha,
        bool ConsenteTermosLgpd,
        PerfilUsuario? Perfil = PerfilUsuario.CLIENTE
    );
}

