using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record CheckPaymentStatusResponse(
    CryptoPaymentStatus Status,
    decimal BitcoinAmount,
    string? TransactionId,
    bool WebshopNotified,
    string? RedirectUrl
);