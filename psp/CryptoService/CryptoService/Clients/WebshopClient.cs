using CryptoService.Clients.Interfaces;
using System.Text;
using System.Text.Json;

namespace CryptoService.Clients;

public class WebshopClient : IWebshopClient
{
    private readonly HttpClient _httpClient;
    private readonly string _webshopUpdateOrder_BaseUrl;

    public WebshopClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _webshopUpdateOrder_BaseUrl = config["WebshopUpdateOrder_BaseUrl"] 
            ?? throw new ArgumentNullException("WebshopUpdateOrder_BaseUrl is missing from appsettings.json");
    }

    public async Task<bool> SendAsync(Guid orderId, bool isCompleted, string pspId, string pspPassword)
    {
        object payload = new
        {
            orderId = orderId,
            orderStatus = isCompleted ? "Completed" : "Cancelled",
            paymentMethod = "Crypto",
            pspId = pspId,
            pspPassword = pspPassword
        };

        string jsonData = JsonSerializer.Serialize(payload);

        using StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(
            $"{_webshopUpdateOrder_BaseUrl}{orderId}", content);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        return true;
    }
}
