namespace Payage.Application.Exceptions
{
    public class RefundAmountExceedsCapturedException : Exception
    {
        public Guid TransactionId { get; }
        public decimal Requested { get; }
        public decimal Remaining { get; }

        public RefundAmountExceedsCapturedException(Guid id, decimal refundAmount, decimal capturedAmout)
        : base($"Refund amount {refundAmount} exceeds remaining captured amount {capturedAmout} for transaction '{id}'.")
        {
            TransactionId = id;
            Requested = refundAmount;
            Remaining = capturedAmout;
        }
    }
}
