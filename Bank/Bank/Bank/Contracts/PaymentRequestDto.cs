using Bank.Models;

public sealed record PaymentRequestDto(
    Guid PaymentRequestId,
    double Amount,
    Currency Currency,
    PaymentRequestStatus Status,
    DateTime ExpiresAt
);