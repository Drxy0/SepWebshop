using MediatR;

namespace CardService.Application.Feature.Payment.Commands.InitPayment
{
    public class InitPayementCommandRequest : IRequest<InitPayementCommandResponse>
    {
        public string MerchantOrderId { get; set; } = string.Empty;
    }

    public class InitPayementCommandResponse
    {
        public string BankUrl { get; set; } = string.Empty;
    }
}
