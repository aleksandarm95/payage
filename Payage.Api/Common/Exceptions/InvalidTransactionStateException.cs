namespace Payage.Api.Common.Exceptions
{
    public class InvalidTransactionStateException : Exception
    {
        public Guid TransactionId { get; }
        public string Status { get; }

        public InvalidTransactionStateException(Guid id, string currentStatus, string newStatus)
         : base($"Cannot change to {newStatus}, for transaction '{id}' while status is '{currentStatus}'.")
        {
            TransactionId = id;
            Status = currentStatus;
        }
    }
}
