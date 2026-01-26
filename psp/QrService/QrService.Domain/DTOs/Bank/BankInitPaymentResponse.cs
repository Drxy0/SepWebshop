namespace QrService.Domain.DTOs.Bank;

public sealed record BankInitPaymentResponse(Guid PaymentRequestId, string PaymentUrl);
