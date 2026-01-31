namespace CryptoService.DTOs;

public sealed record InitializeCryptoPaymentResponse(
    Guid PaymentId,
    string BitcoinAddress,
    decimal BitcoinAmount,
    DateTime ExpiresAt
);
