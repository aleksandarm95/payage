namespace Payage.Api.Common.Exceptions
{
    public class InvalidTransactionStateException : Exception
    {
        public Guid TransactionId { get; }
        public string Status { get; }

        public InvalidTransactionStateException(Guid id, string status, string action)
         : base($"Cannot {action} transaction '{id}' while status is '{status}'.")
        {
            TransactionId = id;
            Status = status;
        }
    }
}
