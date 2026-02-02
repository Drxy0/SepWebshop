using Bank.Models;

namespace Bank.Contracts;

public sealed record GetPaymentRequestResponse(
    Guid PaymentRequestId,
    double Amount,
    Currency Currency,
    PaymentRequestStatus Status,
    DateTime? ExpiresAt,
    string? ErrorMessage
);
