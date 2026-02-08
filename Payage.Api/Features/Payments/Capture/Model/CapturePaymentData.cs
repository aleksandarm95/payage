namespace Payage.Api.Features.Payments.Capture.Model
{
    public class CapturePaymentData
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public decimal CapturedAmount { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
