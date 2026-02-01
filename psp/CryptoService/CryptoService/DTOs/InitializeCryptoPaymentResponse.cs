namespace CryptoService.DTOs;

public sealed record InitializeCryptoPaymentResponse(string QrCodeBase64, Guid MerchantOrderId);
