using Microsoft.AspNetCore.Mvc;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;

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
    }
}
