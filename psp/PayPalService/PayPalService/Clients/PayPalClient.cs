using Microsoft.Extensions.Options;
using PayPalService.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayPalService.Clients;

public sealed class PayPalClient
{
    private readonly HttpClient _http;
    private readonly PayPalSettings _settings;

    public PayPalClient(HttpClient http, IOptions<PayPalSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.Secret}"));

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v1/oauth2/token");

        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

        HttpResponseMessage response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return await _http.SendAsync(request);
    }
}
