using FluentValidation;
using Payage.Api.Features.Payments.Refund.Models;

namespace Payage.Api.Features.Payments.Refund
{
    public class RefundPaymentValidator : AbstractValidator<RefundPaymentRequest>
    {
        public RefundPaymentValidator()
        {
            RuleFor(x => x.Amount)
                .Must(a => a == null || (a > 0 && decimal.Round(a.Value, 2) == a.Value))
                .WithMessage("If Amount is addded, it must be greater than 0 or it must have max 2 decimal places.");

            RuleFor(x => x.Reason)
                .MaximumLength(200);
        }
    }
}
