using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record CheckPaymentStatusResponse(
    Guid PaymentId,
    CryptoPaymentStatus Status,
    decimal BitcoinAmount,
    string? TransactionId,
    int Confirmations,
    bool WebshopNotified
);
