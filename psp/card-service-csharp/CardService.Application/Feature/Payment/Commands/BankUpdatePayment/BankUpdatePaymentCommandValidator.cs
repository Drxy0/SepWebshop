using FluentValidation;

namespace CardService.Application.Feature.Payment.Commands.BankUpdatePayment
{
    public class BankUpdatePaymentCommandValidator : AbstractValidator<BankUpdatePaymentCommandRequest>
    {
        public BankUpdatePaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("PaymentId is required");
            
            RuleFor(x => x.BankId)
                .NotEmpty().WithMessage("BankId is required");
            
            RuleFor(x => x.BankPassword)
                .NotEmpty().WithMessage("BankPassword is required");
        }
    }
}
