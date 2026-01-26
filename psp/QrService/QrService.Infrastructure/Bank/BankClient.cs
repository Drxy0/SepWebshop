using Microsoft.Extensions.Configuration;
using QrService.Application.Abstractions.Bank;
using QrService.Domain.DTOs;
using QrService.Domain.DTOs.Bank;
using System.Net.Http.Json;

namespace QrService.Infrastructure.Bank;

public sealed class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private static readonly Guid QrPspId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // TODO: temp
    private string _backBackendUrl;

    public BankClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _backBackendUrl = configuration["BankBackendUrl"] ?? throw new Exception("BankBackendUrl env variable is missing");
    }

    public async Task<BankInitPaymentResponse> InitQrPaymentAsync(BankInitPaymentRequest payment, CancellationToken cancellationToken)
    {
        DateTime timestamp = DateTime.UtcNow;
        string stan = BankClientHelper.GenerateStan();

        string payload = BankClientHelper.BuildPayload(
            payment.MerchantId,
            payment.Amount,
            payment.Currency,
            stan,
            timestamp);

        string secret = _configuration[$"PSPHmacKey:{QrPspId}"] ?? throw new Exception($"Missing HMAC key for PSP {QrPspId}");
        string signature = BankClientHelper.GenerateHmac(payload, secret);

        var requestDto = new BankInitPaymentRequest(payment.MerchantId, payment.Amount, payment.Currency, stan, timestamp);

        using var request = new HttpRequestMessage(
            method: HttpMethod.Post, 
            requestUri: string.Concat(_backBackendUrl, "/api/bank/Payments/init")
        );

        request.Content = JsonContent.Create(requestDto);

        request.Headers.Add("PspID", QrPspId.ToString());
        request.Headers.Add("Signature", signature);
        request.Headers.Add("Timestamp", timestamp.ToString("O"));
        request.Headers.Add("IsQrPayment", "true");

        // Send request to Bank/api/bank/Payments/init
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Bank init failed: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<BankInitPaymentResponse>(cancellationToken);

        return result ?? throw new Exception("Invalid bank response");
    }

}

