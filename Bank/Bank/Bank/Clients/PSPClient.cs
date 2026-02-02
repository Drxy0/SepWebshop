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

        // Determine PSP base URL and endpoint based on PSP ID
        // 11111111-1111-1111-1111-111111111111 = Card PSP
        // 22222222-2222-2222-2222-222222222222 = QR PSP
        string pspBaseUrl;
        string pspEndpoint;
        
        if (dto.PspId == Guid.Parse("11111111-1111-1111-1111-111111111111"))
        {
            pspBaseUrl = _config["ApiSettings:PspCardBaseUrl"] ?? "https://localhost:7195/";
            pspEndpoint = "ca/Payment/bank/update"; // Card PSP
        }
        else if (dto.PspId == Guid.Parse("22222222-2222-2222-2222-222222222222"))
        {
            pspBaseUrl = _config["ApiSettings:PspQrBaseUrl"] ?? "https://localhost:7075/";
            pspEndpoint = "q/Payment/bank/update"; // QR PSP
        }
        else
        {
            throw new InvalidOperationException($"Unknown PSP ID: {dto.PspId}");
        }

        string fullUrl = $"{pspBaseUrl}{pspEndpoint}/{dto.PspPaymentId}";

        using var response = await _httpClient.PutAsJsonAsync(
            fullUrl,
            requestPayload
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"PSP callback failed: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BankUpdatePaymentCommandResponse>();

        return _config["ApiSettings:WebShopSuccessUrl"]
           ?? "http://localhost:4200/success";
    }
}
