using CryptoService.Models;

namespace CryptoService.DTOs;

public sealed record InitializeCryptoPaymentRequest(Guid MerchantOrderId);
