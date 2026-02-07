using Dapper;
using Microsoft.AspNetCore.Mvc;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Infrastructure.Db;
using System.Threading.Tasks;

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

        [HttpGet("db-ping")]
        public async Task<IActionResult> DbPing([FromServices] IDbConnectionFactory db)
        {
            using var conn = db.Create();
            var result = await conn.ExecuteScalarAsync<int>("SELECT 1;");
            return Ok(new { ok = result == 1 });
        }
    }
}
