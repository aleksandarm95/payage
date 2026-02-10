namespace Payage.Application.Features.Payments.Capture.Models
{
    public record CapturePaymentRequest(
        decimal? Amount
    );
}
