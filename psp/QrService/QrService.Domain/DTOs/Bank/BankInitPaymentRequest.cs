using QrService.Domain.Enums;
using System.Security.Cryptography.X509Certificates;

namespace QrService.Domain.DTOs.Bank;

public sealed record BankInitPaymentRequest(
    string MerchantId,
    Guid PspPaymentId,
    double Amount,
    Currency Currency,
    string Stan,
    DateTime PspTimestamp
);
