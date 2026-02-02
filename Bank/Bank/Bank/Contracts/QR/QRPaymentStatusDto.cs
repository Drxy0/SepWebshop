using Bank.Models;

namespace Bank.Contracts.QR;

public sealed record QrPaymentStatusDto(
    Guid PaymentRequestId,
    PaymentRequestStatus Status,
    double Amount,
    Currency Currency,
    DateTime? ExpiresAt,
    string? TransactionId = null,
    DateTime? CompletedAt = null
);