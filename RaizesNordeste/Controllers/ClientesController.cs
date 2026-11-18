using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesDoNordeste.Aplicacao.Service;
using RaizesNordeste.Domain.Entities;

namespace RaizesNordeste.Api.Controllers
{
    [ApiController]
    [Route("api/v1/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteService _clienteService;

        public ClientesController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cadastrar([FromBody] CriarClienteDto dto)
        {
            var cliente = await _clienteService.CriarClienteAsync(dto);
            return CreatedAtAction(nameof(ConsultarPontos), new { id = cliente.Id }, new
            {
                cliente.Id,
                cliente.Nome,
                cliente.Email,
                cliente.Cpf,
                Perfil = cliente.perfilUsuario.ToString(),
                cliente.Pontos,
                cliente.ConsenteTermosLgpd,
                cliente.DataCriacao
            });
        }

        [HttpGet("{id:guid}/pontos")]
        [Authorize]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConsultarPontos(Guid id)
        {
            var pontos = await _clienteService.ObterSaldoPontosAsync(id);
            return Ok(new { idCliente = id, pontos });
        }


        [HttpDelete("{id:guid}/anonimizar")]
        [Authorize]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Anonimizar(Guid id)
        {
            await _clienteService.AnonimizarClienteAsync(id);
            return Ok(new
            {
                mensagem = "Dados pessoais anonimizados com sucesso conforme diretrizes da LGPD.",
                idCliente = id,
                anonimo = true
            });
        }
    }
}
