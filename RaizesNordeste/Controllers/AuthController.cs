using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaizesDoNordeste.Aplicacao;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesDoNordeste.Aplicacao.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaizesNordeste.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ClienteService _clienteService;
    private readonly IConfiguration _config;

    public AuthController(ClienteService clienteService, IConfiguration config)
    {
        _clienteService = clienteService;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _clienteService.AutenticarAsync(dto);

        var chaveSecreta = _config["Jwt:Key"] ?? "ChavePadraoEmCasosEspecificos!@#";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.perfilUsuario.ToString())
        };

        var tempoExpiracao = 7200; 
        var token = new JwtSecurityToken(
            issuer: "RaizesNordeste.Api",
            audience: "RaizesNordeste.Clientes",
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(tempoExpiracao),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new RespostaLoginDto(
            AccessToken: tokenString,
            TipoToken: "Bearer",
            ExpiraEmSegundos: tempoExpiracao,
            IdUsuario: usuario.Id,
            Nome: usuario.Nome,
            Perfil: usuario.perfilUsuario.ToString()
        ));
    }
}