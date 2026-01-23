using Bank.Models;

namespace Bank.Contracts;

public sealed record QRIpsData(
    Currency Currency,
    decimal Amount,
    string MerchantAccount,
    string MerchantName,
    string Purpose,
    string PaymentCode
);
