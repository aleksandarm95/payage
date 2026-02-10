namespace Payage.Application.Features.Payments.Refund.Models
{
    public record RefundPaymentResponse(
        Guid Id,
        string Status,
        decimal Amount,
        string Currency,
        decimal CapturedAmount,
        decimal RefundedAmount,
        DateTimeOffset UpdatedAt
    );
}
