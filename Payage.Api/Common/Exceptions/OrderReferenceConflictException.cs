namespace Payage.Api.Common.Exceptions
{
    public class OrderReferenceConflictException : Exception
    {
        public string OrderReference { get; }

        public OrderReferenceConflictException(string orderReference, Exception inner)
            : base($"Order reference '{orderReference}' already exists.", inner)
        {
            OrderReference = orderReference;
        }
    }
}
