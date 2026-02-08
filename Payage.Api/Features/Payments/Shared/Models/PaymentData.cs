namespace Payage.Api.Features.Payments.Shared.Models
{
    public class PaymentData
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public decimal CapturedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public string MaskedCardNumber { get; set; } = default!;
        public string CardholderName { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
