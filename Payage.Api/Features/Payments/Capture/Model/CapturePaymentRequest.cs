namespace Payage.Api.Features.Payments.Capture.Model
{
    public record CapturePaymentRequest(
        decimal? Amount
    );
}
