using CryptoService.Clients.Interfaces;
using CryptoService.DTOs.Binance;

namespace CryptoService.Clients;

public sealed class BinanceClient : IBinanceClient
{
    private readonly HttpClient _httpClient;

    public BinanceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool, decimal)> GetBitcoinPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        // Binance public price endpoint
        string url = $"https://api.binance.com/api/v3/ticker/price?symbol={symbol.ToUpper()}";

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "CryptoSchoolProject/1.0");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        BinancePriceDto? result = await response.Content.ReadFromJsonAsync<BinancePriceDto>(cancellationToken);

        if (result is null)
        {
            //Failed to deserialize Binance response
            return (false, 0);
        }

        if (!decimal.TryParse(result.Price, out var price))
        {
            return (false, 0);
        }

        return (true, price);
    }
}
