using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.DTO;
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

        [HttpGet("unidades")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarUnidades()
        {
            var unidades = await _pedidoService.ListarUnidadesAsync();
            return Ok(unidades);
        }

        [HttpGet("cardapio/{idUnidade:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarCardapio(Guid idUnidade)
        {
            
            var cardapio = await _pedidoService.ObterCardapioUnidadeAsync(idUnidade);
            return Ok(cardapio);
            
        }

        [HttpPatch("estoque/{idItem:guid}")]
        [Authorize(Roles = "GERENTE")]
        public async Task<IActionResult> MovimentarEstoque(Guid idItem, [FromBody] MovimentarEstoqueDto dto)
        {
            var ator = User.Identity?.Name ?? "Gerente";
            var item = await _pedidoService.MovimentarEstoqueAsync(idItem, dto, ator);
            return Ok(item);
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListarPedidos(
        [FromQuery] CanalPedido? canalPedido = null,
        [FromQuery] StatusPedido? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
        {
            var resultado = await _pedidoService.ListarPedidosAsync(canalPedido, status, page, limit);
            return Ok(resultado);
        }
        
        [HttpGet("{id:guid}")]
        [Authorize]
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
        [Authorize(Roles = "ATENDENTE,COZINHA,GERENTE")]
        public async Task<IActionResult> AtualizarStatus(
            Guid id,
            [FromQuery] StatusPedido novoStatus,
            [FromQuery] string? motivo = null)
        {
            
            await _pedidoService.AlterarStatusAsync(id, novoStatus, motivo);
            return Ok(new { idPedido = id, statusAtual = novoStatus.ToString(), motivoCancelamento = motivo });
            
        }
    }
}
