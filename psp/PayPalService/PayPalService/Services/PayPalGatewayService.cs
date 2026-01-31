using Microsoft.Extensions.Options;
using PayPalService.Clients;
using PayPalService.Config;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PayPalService.Services;

public sealed class PayPalGatewayService
{
    private readonly PayPalClient _client;
    private readonly PSPDataServiceClient _pspClient;
    private readonly PayPalSettings _settings;

    public PayPalGatewayService(PayPalClient client, PSPDataServiceClient pspClient, IOptions<PayPalSettings> options)
    {
        _client = client;
        _pspClient = pspClient;
        _settings = options.Value;
    }

    public async Task<string> CreateOrderAsync(decimal amount, string currency)
    {
        string token = await _client.GetAccessTokenAsync();

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    amount = new
                    {
                        currency_code = currency,
                        value = amount.ToString("F2", CultureInfo.InvariantCulture)
                    }
                }
            },
            application_context = new
            {
                return_url = _settings.ReturnUrl,
                cancel_url = _settings.CancelUrl
            }
        });

        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        string? approvalUrl = json.RootElement
            .GetProperty("links")
            .EnumerateArray()
            .First(l => l.GetProperty("rel").GetString() == "approve")
            .GetProperty("href")
            .GetString();

        return approvalUrl!;
    }

    public async Task CaptureAsync(string orderId)
    {
        string token = await _client.GetAccessTokenAsync();

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders/{orderId}/capture");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        JsonDocument payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        await _pspClient.StoreTransactionAsync(new
        {
            Provider = "PayPal",
            OrderId = orderId,
            Raw = payload.RootElement.ToString(),
            Status = "Completed"
        });
    }
}
