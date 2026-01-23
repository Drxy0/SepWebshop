using Bank.Models;

namespace Bank.Contracts;

public sealed record PaymentRequestDto(
    Guid PaymentRequestId,
    decimal Amount,
    Currency Currency,
    PaymentRequestStatus Status,
    DateTime ExpiresAt
);
