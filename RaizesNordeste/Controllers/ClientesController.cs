using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.Service;

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
        public async Task<IActionResult> Cadastrar([FromBody] CriarClienteDto dto)
        {
            try
            {
                var cliente = await _clienteService.CriarClienteAsync(dto);
                return CreatedAtAction(nameof(ConsultarPontos), new { id = cliente.Id }, cliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("{id:guid}/pontos")]
        public async Task<IActionResult> ConsultarPontos(Guid id)
        {
            try
            {
                var pontos = await _clienteService.ObterSaldoPontosAsync(id);
                return Ok(new { idCliente = id, pontos });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/anonimizar")]
        public async Task<IActionResult> Anonimizar(Guid id)
        {
            try
            {
                await _clienteService.AnonimizarClienteAsync(id);
                return Ok(new
                {
                    mensagem = "Dados pessoais anonimizados com sucesso conforme diretrizes da LGPD.",
                    idCliente = id,
                    anonimo = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
