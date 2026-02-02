using FluentValidation;

namespace CardService.Application.Feature.Payment.Commands.InitPayment
{
    public class InitPayementCommandValidator : AbstractValidator<InitPayementCommandRequest>
    {
        public InitPayementCommandValidator()
        {
            RuleFor(x => x.MerchantOrderId)
                .NotEmpty().WithMessage("MerchantOrderId is required");
        }
    }
}
