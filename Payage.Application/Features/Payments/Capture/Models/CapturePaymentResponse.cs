namespace Payage.Application.Features.Payments.Capture.Models
{
    public record CapturePaymentResponse(
        Guid Id,
        string Status,
        decimal Amount,
        string Currency,
        decimal CapturedAmount,
        DateTimeOffset UpdatedAt
    );
}
