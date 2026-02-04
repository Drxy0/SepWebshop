namespace PayPalService.DTOs;

public sealed record PayPalCreateOrderResult(string PayPalOrderId, string ApprovalUrl);
