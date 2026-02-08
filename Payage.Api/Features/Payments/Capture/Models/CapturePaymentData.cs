namespace Payage.Api.Features.Payments.Capture.Models
{
    public class CapturePaymentData
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal CapturedAmount { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
