using MediatR;

namespace QrService.Application.Feature.Payment.Commands.BankUpdatePayment
{
    public class BankUpdatePaymentCommandRequest : IRequest<BankUpdatePaymentCommandResponse>
    {
        public Guid PaymentId { get; set; }
        public string BankId { get; set; } = string.Empty;
        public string BankPassword { get; set; } = string.Empty;
    }
    public class BankUpdatePaymentCommandResponse
    {
        public string Status { get; set; } = string.Empty;
    }
}
