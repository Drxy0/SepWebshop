namespace PayPalService.Models;

public class PayPalPayment
{
    public Guid Id { get; set; } // just internal Id, not used anywhere
    public Guid MerchantOrderId { get; set; }
    public required string PayPalOrderId { get; set; }
    public DateTime CreatedAt { get; set; }     // extra fluff
}
