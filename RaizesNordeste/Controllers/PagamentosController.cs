using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaizesDoNordeste.Aplicacao.DTO;
using RaizesDoNordeste.Aplicacao.Service;
using RaizesNordeste.Domain.Entities;

namespace RaizesNordeste.Api.Controllers
{
    [ApiController]
    [Route("api/v1/pagamentos")]
    public class PagamentosController : ControllerBase
    {
        private readonly PedidoService _pedidoService;
        private readonly AuditoriaService _auditoriaService;

        public PagamentosController(PedidoService pedidoService, AuditoriaService auditoriaService)
        {
            _pedidoService = pedidoService;
            _auditoriaService = auditoriaService;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ProcessarWebhook([FromBody] WebhookPagamentoDto dto)
        {
           
            await _pedidoService.ProcessarConfirmacaoPagamentoAsync(dto.IdPedido, dto.Aprovado, dto.IdTransacao);
            return Ok(new
            {
                mensagem = "Notificação de pagamento processada com sucesso.",
                statusProcessado = dto.Aprovado ? "RECEBIDO" : "PAGAMENTO_RECUSADO"
            });
           
        }

        [HttpGet("auditoria")]
        [Authorize(Roles = "GERENTE")]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErroPadraoDto), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListarAuditoria([FromQuery] Guid? idUnidade = null)
        {
            var logs = await _auditoriaService.ConsultarRelatorioAsync(idUnidade);
            return Ok(logs);
        }
    }
}
