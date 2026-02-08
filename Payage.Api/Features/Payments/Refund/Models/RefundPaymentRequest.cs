namespace Payage.Api.Features.Payments.Refund.Models
{
    public record RefundPaymentRequest(
        decimal? Amount,
        string? Reason
    );
}
