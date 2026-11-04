using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.Service;
using RaizesNordeste.Domain.Enums;

namespace RaizesNordeste.Api.Controllers
{
    [ApiController]
    [Route("api/v1/pedidos")]
    public class PedidosController : ControllerBase
    {
        private readonly PedidoService _pedidoService;

        public PedidosController(PedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpGet("cardapio/{idUnidade:guid}")]
        public async Task<IActionResult> ListarCardapio(Guid idUnidade)
        {
            try
            {
                var cardapio = await _pedidoService.ObterCardapioUnidadeAsync(idUnidade);
                return Ok(cardapio);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RealizarPedido([FromBody] CriarPedidoDto dto)
        {
            try
            {
                var pedido = await _pedidoService.CriarPedidoAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            try
            {
                var pedido = await _pedidoService.ObterPorIdAsync(id);
                return Ok(pedido);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> AtualizarStatus(
            Guid id,
            [FromQuery] StatusPedido novoStatus,
            [FromQuery] string? motivo = null)
        {
            try
            {
                await _pedidoService.AlterarStatusAsync(id, novoStatus, motivo);
                return Ok(new { idPedido = id, statusAtual = novoStatus.ToString(), motivoCancelamento = motivo });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
