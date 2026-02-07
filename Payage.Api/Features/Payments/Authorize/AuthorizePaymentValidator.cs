using FluentValidation;
using Payage.Api.Features.Payments.Authorize.Models;

namespace Payage.Api.Features.Payments.Authorize
{
    public class AuthorizePaymentValidator : AbstractValidator<AuthorizePaymentRequest>
    {
        public AuthorizePaymentValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .Must(a => decimal.Round(a, 2) == a)
                .WithMessage("Amount must be greater than 0 and have max 2 decimal places.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .Matches("^[A-Z]{3}$")
                .WithMessage("Currency must be a 3-letter ISO 4217 code.");

            RuleFor(x => x.CardholderName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100)
                .WithMessage("Cardholder name must have length between 2 and 100.");

            RuleFor(x => x.OrderReference)
                .NotEmpty()
                .MaximumLength(50)
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("OrderReference must be alfanumeric");

            RuleFor(x => x.CardNumber)
                .NotEmpty()
                .Must(BeValidPan)
                .WithMessage("CardNumber must be 13-19 digits and pass Luhn check.");

            RuleFor(x => x.Cvv)
                .NotEmpty()
                .Matches("^\\d{3,4}$")
                .WithMessage("Cvv must be 3 or 4 digits.");

            RuleFor(x => x.ExpirationYear)
                .InclusiveBetween(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 20)
                .WithMessage("Card is expired.");
        }

        private bool BeValidPan(string cardNumber)
        {
            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
            if (digits.Length < 13 || digits.Length > 19) 
                return false;
            return PassesLuhn(digits);
        }

        private bool PassesLuhn(string digits)
        {
            var sum = 0;
            var alternate = false;

            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

    }
}
