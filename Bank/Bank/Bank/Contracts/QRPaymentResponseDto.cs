namespace Bank.Contracts;

public sealed record QRPaymentResponseDto(
    Guid PaymentRequestId,
    string QrCodeBase64
);
