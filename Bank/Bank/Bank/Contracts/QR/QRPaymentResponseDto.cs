using Bank.Models;

namespace Bank.Contracts.QR;

public sealed record QRPaymentResponseDto(
    Guid PaymentRequestId,
    string QrCodeBase64,
    PaymentRequestStatus Status,
    string STAN,
    DateTime? ExpiresAt = null  // for frontend countdown
);
