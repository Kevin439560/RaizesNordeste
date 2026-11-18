using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesDoNordeste.Aplicacao.Service;
using RaizesNordeste.Domain.Entities;
using RaizesNordeste.Domain.Enums;
using RaizesNordeste.Dominio.Enums;

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
        [ProducesResponseType(typeof(List<Unidade>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarUnidades()
        {
            var unidades = await _pedidoService.ListarUnidadesAsync();
            return Ok(unidades);
        }

        [HttpGet("cardapio/{idUnidade:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<Item>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListarCardapio(Guid idUnidade)
        {
            
            var cardapio = await _pedidoService.ObterCardapioUnidadeAsync(idUnidade);
            return Ok(cardapio);
            
        }

        [HttpPatch("estoque/{idItem:guid}")]
        [Authorize(Roles = "GERENTE")]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> MovimentarEstoque(Guid idItem, [FromBody] MovimentarEstoqueDto dto)
        {
            var ator = User.Identity?.Name ?? "Gerente";
            var item = await _pedidoService.MovimentarEstoqueAsync(idItem, dto, ator);
            return Ok(item);
        }     

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RealizarPedido([FromBody] CriarPedidoDto dto)
        {
            
            var pedido = await _pedidoService.CriarPedidoAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = pedido.Id }, pedido);
            
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PaginacaoPedidosDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListarPedidos(
       [FromQuery] Guid? idCliente = null,
       [FromQuery] CanalPedido? canalPedido = null,
       [FromQuery] StatusPedido? status = null,
       [FromQuery] int page = 1,
       [FromQuery] int limit = 10) {
            var perfilGerente = User.IsInRole(nameof(PerfilUsuario.GERENTE));

            if (!idCliente.HasValue && perfilGerente) {
                return Forbid();
            }

            var idUsuarioLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!perfilGerente && idCliente.HasValue && idUsuarioLogado != idCliente.Value.ToString()) {
                return Forbid();
            }

            var resultado = await _pedidoService.ListarPedidosAsync(idCliente, canalPedido, status, page, limit);
            return Ok(resultado);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            
            var pedido = await _pedidoService.ObterPorIdAsync(id);
            return Ok(pedido);
       
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "ATENDENTE,COZINHA,GERENTE")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AtualizarStatus(
            Guid id,
            [FromQuery] StatusPedido? novoStatus,
            [FromQuery] string? motivo = null)
        {

            
            await _pedidoService.AlterarStatusAsync(id, novoStatus, motivo);
            return Ok(new { idPedido = id, statusAtual = novoStatus.ToString(), motivoCancelamento = motivo });
            
        }
    }
}
