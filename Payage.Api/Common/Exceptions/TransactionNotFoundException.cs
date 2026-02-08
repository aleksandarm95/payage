namespace Payage.Api.Common.Exceptions
{
    public class TransactionNotFoundException : Exception
    {
        public Guid TransactionId { get; }
        public TransactionNotFoundException(Guid id) 
            : base($"Transaction '{id}' was not found.") 
        {
            TransactionId = id;
        }
    }
}
