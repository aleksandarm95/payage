namespace Payage.Api.Features.Payments.Capture.Model
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
