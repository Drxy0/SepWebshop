using Bank.Contracts;
using Bank.Models;

namespace Bank.Clients;

public class PspClient : IPSPClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public PspClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> NotifyPaymentStatusAsync(PspPaymentStatusDto dto)
    {
        var requestPayload = new
        {
            PaymentId = dto.PspPaymentId,
            BankId = _config["Bank:BankId"],
            BankPassword = _config["Bank:BankPassword"]
        };

        using var response = await _httpClient.PutAsJsonAsync($"q/Payment/bank/update/{dto.PspPaymentId}", requestPayload);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"PSP callback failed: {errorMessage}");
        }

        var result = await response.Content.ReadFromJsonAsync<BankUpdatePaymentCommandResponse>();

        return _config["ApiSettings:WebShopSuccessUrl"]!;
    }
}
