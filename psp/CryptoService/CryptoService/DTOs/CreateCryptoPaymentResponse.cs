namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentResponse(
    Guid PaymentId,
    string BitcoinAddress,
    decimal BitcoinAmount,
    string Network,      // "testnet"
    DateTime ExpiresAt
);
