using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record CheckPaymentStatusResponse(
    Guid MerchantOrderId,
    CryptoPaymentStatus Status,
    bool WebshopNotified,
    string? redirectUrl
);
