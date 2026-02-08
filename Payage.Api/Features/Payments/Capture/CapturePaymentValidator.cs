using FluentValidation;
using Payage.Api.Features.Payments.Capture.Model;

namespace Payage.Api.Features.Payments.Capture
{
    public class CapturePaymentValidator : AbstractValidator<CapturePaymentRequest>
    {
        public CapturePaymentValidator()
        {
            RuleFor(x => x.Amount)
                .Must(a => a is null || (a > 0 && decimal.Round(a.Value, 2) == a.Value))
                .WithMessage("If Amount is addded, it must be greater than 0 or it must have max 2 decimal places.");
        }
    }
}
