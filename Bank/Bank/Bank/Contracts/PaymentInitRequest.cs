using Bank.Models;

namespace Bank.Contracts;

public sealed record PaymentInitRequest(
    string MerchantId,
    double Amount,
    Currency Currency,
    string Stan,
    DateTime PspTimestamp
);
