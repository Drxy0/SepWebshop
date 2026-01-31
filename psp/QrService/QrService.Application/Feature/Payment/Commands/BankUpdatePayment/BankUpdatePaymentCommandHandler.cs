using MediatR;
using QrService.Domain.Contracts;

namespace QrService.Application.Feature.Payment.Commands.BankUpdatePayment
{
    public class BankUpdatePaymentCommandHandler : IRequestHandler<BankUpdatePaymentCommandRequest, BankUpdatePaymentCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        public BankUpdatePaymentCommandHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }
        public Task<BankUpdatePaymentCommandResponse> Handle(BankUpdatePaymentCommandRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
