namespace PayPalService.Clients.Interfaces;

/// <summary>
/// Informs the webshop backend that payment has been processed
/// </summary>
public interface IWebshopClient
{
    Task<bool> SendAsync(Guid orderId, bool isCompleted);
}
