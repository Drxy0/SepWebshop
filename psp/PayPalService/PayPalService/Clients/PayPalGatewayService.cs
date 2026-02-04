using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PayPalService.Clients.Interfaces;
using PayPalService.Config;
using PayPalService.DTOs;
using PayPalService.Models;
using PayPalService.Persistance;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayPalService.Clients;

public sealed class PayPalGatewayService
{
    private readonly PayPalClient _client;
    private readonly PayPalSettings _settings;
    private readonly IWebshopClient _webshopClient;
    private readonly PayPalDbContext _context;

    private readonly string _webshopSuccessUrl;

    public PayPalGatewayService(PayPalClient client, IOptions<PayPalSettings> options, IConfiguration config, IWebshopClient webshopClient, PayPalDbContext context)
    {
        _client = client;
        _settings = options.Value;

        _webshopSuccessUrl = config["ApiSettings:WebShopSuccessUrl"] ?? throw new Exception("ApiSettings:WebShopSuccessUrl is missing from appsettings.json");
        _webshopClient = webshopClient;
        _context = context;
    }

    public async Task<PayPalCreateOrderResult> CreateOrderAsync(double amount, string currency, Guid merchantOrderId)
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
                    custom_id = merchantOrderId,
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

        string paypalOrderId = json.RootElement.GetProperty("id").GetString()!;

        string? approvalUrl = json.RootElement
            .GetProperty("links")
            .EnumerateArray()
            .First(l => l.GetProperty("rel").GetString() == "approve")
            .GetProperty("href")
            .GetString();

        return new PayPalCreateOrderResult(paypalOrderId, approvalUrl);
    }

    public async Task<(bool success, string redirectUrl)> CaptureAsync(string orderId)
    {
        try
        {
            string token = await _client.GetAccessTokenAsync();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/checkout/orders/{orderId}/capture");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");


            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            JsonDocument payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            PayPalPayment? payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PayPalOrderId == orderId);

            if (payment == null)
            {
                return (false, null!);
            }

            bool notifySuccess = await _webshopClient.SendAsync(payment.MerchantOrderId, true);

            return (notifySuccess, _webshopSuccessUrl);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Capture failed for order {orderId}: {ex.Message}");
            return (false, null);
        }
    }
}
