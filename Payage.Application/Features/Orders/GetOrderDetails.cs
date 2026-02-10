namespace Payage.Application.Features.Orders
{
    internal class GetOrderDetails
    {
        public record Query(/*TODO: Add query properties. */);

        public class Handler
        {
            public Task HandleAsync(Query query, CancellationToken ct) =>
                throw new NotImplementedException();
        }
    }
}
