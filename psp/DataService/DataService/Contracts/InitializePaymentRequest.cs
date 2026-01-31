using DataService.Models;

namespace DataService.Contracts;

public sealed record InitializePaymentRequest(
    string MerchantId,
    string MerchantPassword,
    double Amount,
    Currency currency,
    Guid MerchantOrderId,
    DateTime MerchantTimestamp);