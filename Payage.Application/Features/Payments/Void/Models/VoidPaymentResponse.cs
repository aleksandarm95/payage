namespace Payage.Application.Features.Payments.Void.Models
{
    public record VoidPaymentResponse(
        Guid Id,
        string Status,
        decimal Amount,
        string Currency,
        DateTimeOffset UpdatedAt
    );
}
