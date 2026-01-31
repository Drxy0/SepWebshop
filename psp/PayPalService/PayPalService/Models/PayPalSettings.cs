namespace PayPalService.Models;

public sealed class PayPalSettings
{
    public required string BaseUrl { get; set; }
    public required string ClientId { get; set; }
    public required string Secret { get; set; }
    public required string ReturnUrl { get; set; }
    public required string CancelUrl { get; set; }
}
