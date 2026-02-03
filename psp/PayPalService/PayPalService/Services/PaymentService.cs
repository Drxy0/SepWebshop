using PayPalService.Clients;
using PayPalService.DTOs;
using PayPalService.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPalService.Services;

public class PaymentService : IPaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalGatewayService _gatewayService;

    public PaymentService(IHttpClientFactory httpClientFactory, PayPalGatewayService gatewayService)
    {
        _httpClientFactory = httpClientFactory;
        _gatewayService = gatewayService;
    }

    public async Task<InitializePaymentResponse?> CreatePaymentAsync(InitializePaymentRequest request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("DataServiceClient");
        var response = await client.GetAsync($"d/Payments/{request.MerchantOrderId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Payment details not found in DataService.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        DataServicePaymentResponse? paymentData = await response.Content.ReadFromJsonAsync<DataServicePaymentResponse>(options, cancellationToken);

        if (paymentData == null)
            throw new Exception("Failed to deserialize payment data.");

        string approvalUrl = await _gatewayService.CreateOrderAsync(paymentData.Amount, paymentData.Currency.ToString());

        return new InitializePaymentResponse(paymentData.MerchantOrderId, approvalUrl);
    }
}
