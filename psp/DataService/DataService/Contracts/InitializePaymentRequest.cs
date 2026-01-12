using DataService.Models;

namespace DataService.Contracts;

public sealed record InitializePaymentRequest(
    string MerchantId,
    string MerchantPassword,
    double Amount,
    Currency currency,
    string MerchantOrderId,
    DateTime MerchantTimestamp);