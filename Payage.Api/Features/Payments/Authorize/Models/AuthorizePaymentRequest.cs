namespace Payage.Api.Features.Payments.Authorize.Models
{
    public record AuthorizePaymentRequest(
        decimal Amount,
        string Currency,
        string CardNumber,
        string CardholderName,
        int ExpirationMonth,
        int ExpirationYear,
        string Cvv,
        string OrderReference
    );
}
