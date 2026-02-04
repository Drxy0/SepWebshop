using Microsoft.EntityFrameworkCore;
using PayPalService.Clients;
using PayPalService.DTOs;
using PayPalService.Models;
using PayPalService.Persistance;
using PayPalService.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayPalService.Services;

public class PaymentService : IPaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PayPalGatewayService _gatewayService;
    private readonly PayPalDbContext _context;

    public PaymentService(IHttpClientFactory httpClientFactory, PayPalGatewayService gatewayService, PayPalDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _gatewayService = gatewayService;
        _context = context;
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

        PayPalCreateOrderResult paypalResult = await _gatewayService.CreateOrderAsync(paymentData.Amount, paymentData.Currency.ToString(), paymentData.MerchantOrderId);

        await _context.Payments.AddAsync(new PayPalPayment
        {
            MerchantOrderId = paymentData.MerchantOrderId,
            PayPalOrderId = paypalResult.PayPalOrderId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new InitializePaymentResponse(paymentData.MerchantOrderId, paypalResult.ApprovalUrl);
    }
    public async Task<PayPalPayment?> GetByPayPalIdAsync(string paypalOrderId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.PayPalOrderId == paypalOrderId);
    }
}
