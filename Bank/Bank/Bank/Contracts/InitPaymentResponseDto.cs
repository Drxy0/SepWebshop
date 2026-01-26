namespace Bank.Contracts;

public sealed record InitPaymentResponseDto(Guid PaymentRequestId, string PaymentUrl);
