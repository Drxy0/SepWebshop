using Bank.Models;

namespace Bank.Contracts.QR;

public sealed record QRPaymentResponseDto(
    Guid PaymentRequestId,
    string QrCodeBase64,
    PaymentRequestStatus Status = PaymentRequestStatus.Pending,
    string? TransactionReference = null,  // (STAN or IPS transaction ID)
    DateTime? ExpiresAt = null  // for frontend countdown
);
