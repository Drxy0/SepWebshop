using Bank.Models;

namespace Bank.Contracts;

public sealed record PaymentInitRequest(
    Guid MerchantId,
    decimal Amount,
    Currency Currency,
    string Stan,
    DateTime PspTimestamp
);
