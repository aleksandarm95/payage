using Microsoft.AspNetCore.Mvc;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Features.Payments.Capture;
using Payage.Api.Features.Payments.Capture.Model;

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
        [ProducesResponseType(typeof(AuthorizePaymentResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<CapturePaymentResponse>> Capture([FromRoute] Guid id, [FromBody] CapturePaymentRequest capturePaymentRequest, [FromServices] CapturePaymentHandler capturePaymentHandler,
            CancellationToken cancellationToken)
        {
            var response = await capturePaymentHandler.HandleAsync(id, capturePaymentRequest, cancellationToken);

            return Ok(response);
        }
    }
}
