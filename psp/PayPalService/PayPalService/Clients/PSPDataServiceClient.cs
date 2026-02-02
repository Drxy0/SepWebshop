using Microsoft.Extensions.Options;
using PayPalService.Config;

namespace PayPalService.Clients;

public sealed class PSPDataServiceClient
{
    private readonly HttpClient _http;
    private readonly PSPDataServiceSettings _settings;

    public PSPDataServiceClient(HttpClient http, IOptions<PSPDataServiceSettings> options)
    {
        _http = http;
        _settings = options.Value;
    }

    public async Task StoreTransactionAsync(object payload)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync(_settings.BaseUrl, payload);
        response.EnsureSuccessStatusCode();
    }
}
