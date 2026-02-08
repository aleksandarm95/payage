using Microsoft.AspNetCore.Mvc;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Features.Payments.Capture;
using Payage.Api.Features.Payments.Capture.Models;
using Payage.Api.Features.Payments.Refund;
using Payage.Api.Features.Payments.Refund.Models;
using Payage.Api.Features.Payments.Shared;
using Payage.Api.Features.Payments.Shared.Models;
using Payage.Api.Features.Payments.Void;
using Payage.Api.Features.Payments.Void.Models;

namespace Payage.Api.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    public class PaymentsController : ControllerBase
    {
        [HttpPost("authorize")]
        [ProducesResponseType(typeof(AuthorizePaymentResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult<AuthorizePaymentResponse>> Authorize([FromBody] AuthorizePaymentRequest authorizePaymentRequest, [FromServices] AuthorizePaymentHandler authorizePaymentHandler,
            CancellationToken cancellationToken)
        {
            var response = await authorizePaymentHandler.HandleAsync(authorizePaymentRequest, cancellationToken);

            return CreatedAtAction(nameof(Authorize), response);
        }

        [HttpPost("{id}/capture")]
        [ProducesResponseType(typeof(CapturePaymentResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<CapturePaymentResponse>> Capture([FromRoute] Guid id, [FromBody] CapturePaymentRequest capturePaymentRequest, [FromServices] CapturePaymentHandler capturePaymentHandler,
            CancellationToken cancellationToken)
        {
            var response = await capturePaymentHandler.HandleAsync(id, capturePaymentRequest, cancellationToken);

            return Ok(response);
        }

        [HttpPost("{id:guid}/void")]
        [ProducesResponseType(typeof(VoidPaymentResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<VoidPaymentResponse>> Void([FromRoute] Guid id, [FromServices] VoidPaymentHandler voidPaymentHandler,
             CancellationToken cancellationToken)
        {
            var response = await voidPaymentHandler.HandleAsync(id, cancellationToken);
            return Ok(response);
        }

        [HttpPost("{id}/refund")]
        [ProducesResponseType(typeof(RefundPaymentResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<RefundPaymentResponse>> Refund([FromRoute] Guid id, [FromBody] RefundPaymentRequest refundPaymentRequest, [FromServices] RefundPaymentHandler refundPaymentHandler,
            CancellationToken cancellationToken)
        {
            var response = await refundPaymentHandler.HandleAsync(id, refundPaymentRequest, cancellationToken);

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentData>> GetById([FromRoute] Guid id, [FromServices] PaymentHandler handler, CancellationToken cancellationToken)
        {
            var response = await handler.HandleAsync(id, cancellationToken);
            return Ok(response);
        }
    }
}
