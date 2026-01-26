using QrService.Domain.Enums;

namespace QrService.Domain.DTOs.Bank;

public sealed record BankInitPaymentRequest(
    string MerchantId,
    double Amount,
    Currency Currency,
    string Stan,
    DateTime PspTimestamp
);
