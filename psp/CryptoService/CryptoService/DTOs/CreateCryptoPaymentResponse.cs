namespace CryptoService.DTOs;

public sealed record CreateCryptoPaymentResponse(
    Guid PaymentId,
    string BitcoinAddress,
    decimal BitcoinAmount,
<<<<<<< HEAD
=======
    string Network,      // "testnet"
>>>>>>> 5cbd7fe (Add base implementation)
    DateTime ExpiresAt
);
