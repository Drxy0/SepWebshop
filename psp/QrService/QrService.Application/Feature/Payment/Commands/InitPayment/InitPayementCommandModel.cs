using MediatR;

namespace QrService.Application.Feature.Payment.Commands.InitPayment
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
