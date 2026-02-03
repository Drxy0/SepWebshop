namespace PayPalService.DTOs;

public sealed record InitializePaymentResponse(Guid MerchantOrderId, string ApprovalUrl);
