using Bank.Contracts;
using Bank.Contracts.QR;

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

    Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId);

    // For simulation - manually trigger QR payment
    Task<ProcessQrPaymentResponse> ProcessQrPayment(Guid paymentRequestId, string? customerAccountNumber);

    // Frontend polls this endpoint to check payment status
    Task<QrPaymentStatusDto> GetQrPaymentStatus(Guid paymentRequestId);
}