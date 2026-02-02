using Bank.Models;

namespace Bank.Contracts.QR;

public sealed record QRIpsData(
    Currency Currency,
    double Amount,
    string MerchantAccount,
    string MerchantName,
    string Purpose,
    string PaymentCode,
    string? Stan = null  // payment reference (STAN)
);
