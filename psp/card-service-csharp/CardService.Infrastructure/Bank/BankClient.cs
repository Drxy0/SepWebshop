using Microsoft.Extensions.Configuration;
using CardService.Application.Abstractions.Bank;
using CardService.Domain.DTOs;
using System.Net.Http.Json;

namespace CardService.Infrastructure.Bank
{
    public sealed class BankClient : IBankClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private static readonly Guid CardPspId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private string _bankBackendUrl;

        public BankClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _bankBackendUrl = configuration["ApiSettings:BankBackendUrl"] ?? throw new Exception("BankBackendUrl env variable is missing");
        }

        public async Task<BankInitPaymentResponse> InitCardPaymentAsync(BankInitPaymentRequest payment, CancellationToken cancellationToken)
        {
            DateTime timestamp = DateTime.UtcNow;
            string stan = BankClientHelper.GenerateStan();

            string payload = BankClientHelper.BuildPayload(
                payment.MerchantId,
                payment.Amount,
                payment.Currency,
                stan,
                timestamp);

            string secret = _configuration[$"PSPHmacKey:{CardPspId}"] ?? throw new Exception($"Missing HMAC key for PSP {CardPspId}");
            string signature = BankClientHelper.GenerateHmac(payload, secret);

            var requestDto = new BankInitPaymentRequest(
                payment.MerchantId, 
                payment.PspPaymentId, 
                payment.Amount, 
                payment.Currency, 
                stan, 
                timestamp);

            using var request = new HttpRequestMessage(
                method: HttpMethod.Post,
                requestUri: string.Concat(_bankBackendUrl, "/api/bank/Payments/init")
            );

            request.Content = JsonContent.Create(requestDto);

            request.Headers.Add("PspID", CardPspId.ToString());
            request.Headers.Add("Signature", signature);
            request.Headers.Add("Timestamp", timestamp.ToString("O"));
            request.Headers.Add("IsCardPayment", "true");

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
}
