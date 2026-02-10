namespace Payage.Application.Exceptions
{
    public class CaptureAmountExceedsAuthorizedException : Exception
    {
        public Guid TransactionId { get; }
        public decimal Requested { get; }
        public decimal Remaining { get; }

        public CaptureAmountExceedsAuthorizedException(Guid id, decimal requestedAmount, decimal remainingAmount)
        : base($"Capture amount {requestedAmount} exceeds remaining authorized amount {remainingAmount} for transaction '{id}'.")
        {
            TransactionId = id;
            Requested = requestedAmount;
            Remaining = remainingAmount;
        }
    }
}
