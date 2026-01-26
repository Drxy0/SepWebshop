using Bank.Models;

namespace Bank.Contracts;

public sealed record PaymentRequestDto(
    Guid PaymentRequestId,
    double Amount,
    Currency Currency,
    PaymentRequestStatus Status,
    DateTime ExpiresAt
);
