namespace Payage.Api.Features.Payments.Capture.Models
{
    public record CapturePaymentRequest(
        decimal? Amount
    );
}
