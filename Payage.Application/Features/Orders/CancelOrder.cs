using FluentValidation;

namespace Payage.Application.Features.Orders
{
    internal class CancelOrder
    {
        public record Command(/*TODO: Add record properties. */);

        public class Handler
        {
            public Task HandleAsync(Command cmd, CancellationToken ct) =>
                throw new NotImplementedException();
        }

        public class Validator : AbstractValidator<Command>
        {
            //TODO: Add validation rules for the command properties when they are added.
        }
    }
}
