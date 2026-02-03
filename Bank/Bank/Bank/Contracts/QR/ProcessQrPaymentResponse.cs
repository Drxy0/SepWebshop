using Bank.Models;

namespace Bank.Contracts.QR;

public sealed record ProcessQrPaymentResponse(
    Guid PaymentRequestId,
    PaymentRequestStatus Status,
    string? RedirectUrl = null
);
