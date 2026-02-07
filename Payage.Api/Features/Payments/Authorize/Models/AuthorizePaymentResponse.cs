namespace Payage.Api.Features.Payments.Authorize.Models
{
    public record AuthorizePaymentResponse(
        Guid Id,
        string Status,
        decimal Amount,
        string Currency,
        string MaskedCardNumber,
        DateTimeOffset CreatedAt
    );
}
