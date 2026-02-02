namespace Bank.Contracts;

public sealed record InitializePaymentServiceResult(InitializePaymentResult Result, InitPaymentResponseDto? Response);
