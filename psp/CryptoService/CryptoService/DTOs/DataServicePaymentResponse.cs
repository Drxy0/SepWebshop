using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record DataServicePaymentResponse(
    Guid Id,
    string MerchantId,
    string MerchantPassword,
    double Amount,
    Currency Currency,
    string MerchantOrderId,
    DateTime MerchantTimestamp
);

