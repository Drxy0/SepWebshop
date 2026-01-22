using Bank.Contracts;

namespace Bank.Services.Interfaces;

public interface IPaymentService
{
    Task<InitializePaymentServiceResult> InitializePayment(
        PaymentInitRequest request,
        Guid pspId,
        string signature,
        DateTime timestamp,
        bool isQrPayment);

    Task<string> ExecuteCardPayment(Guid paymentRequestId, PayByCardRequest request);

    Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId);

}
