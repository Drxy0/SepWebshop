namespace PayPalService.DTOs;

public sealed record DataServicePaymentResponse(
    Guid Id,
    string MerchantId,
    string MerchantPassword,
    double Amount,
    string Currency,
    Guid MerchantOrderId,
    DateTime MerchantTimestamp
);
