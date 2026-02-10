using FluentValidation;
using Payage.Application.Features.Payments.Capture.Models;

namespace Payage.Application.Features.Payments.Capture
{
    public class CapturePaymentValidator : AbstractValidator<CapturePaymentRequest>
    {
        public CapturePaymentValidator()
        {
            RuleFor(x => x.Amount)
                .Must(a => a == null || (a > 0 && decimal.Round(a.Value, 2) == a.Value))
                .WithMessage("If Amount is addded, it must be greater than 0 or it must have max 2 decimal places.");
        }
    }
}
