using Microsoft.Extensions.Options;
using SepWebshop.Application.Abstractions.Payment;
using SepWebshop.Domain.Orders;
using System.Net.Http.Json;

namespace SepWebshop.Infrastructure.Payment;

public sealed class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly PspOptions _pspOptions;

    public PaymentService(HttpClient httpClient, IOptions<PspOptions> pspOptions)
    {
        _httpClient = httpClient;
        _pspOptions = pspOptions.Value;
    }

    public async Task<PaymentInitializationResult> InitializePaymentAsync(
        string merchantId,
        string merchantPassword,
        double amount,
        Currency currency,
        Guid merchantOrderId,
        DateTime merchantTimestamp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            PaymentInitRequest request = new PaymentInitRequest
            {
                MerchantId = merchantId,
                MerchantPassword = merchantPassword,
                Amount = (double)amount,
                Currency = currency,
                MerchantOrderId = merchantOrderId,
                MerchantTimestamp = merchantTimestamp
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                _pspOptions.PaymentInitEndpoint,
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                return new PaymentInitializationResult(PaymentId: null, IsSuccess: false);
            }

            PaymentInitResponse? result = await response.Content.ReadFromJsonAsync<PaymentInitResponse>(
                cancellationToken: cancellationToken);

            return new PaymentInitializationResult(PaymentId: result?.PaymentId, IsSuccess: true);
        }
        catch (HttpRequestException)
        {
            return new PaymentInitializationResult(PaymentId: null, IsSuccess: false);
        }
        catch (Exception)
        {
            return new PaymentInitializationResult(PaymentId: null, IsSuccess: false);
        }
    }

}

// DTOs for external API
internal sealed record PaymentInitRequest
{
    public required string MerchantId { get; init; }
    public required string MerchantPassword { get; init; }
    public required double Amount { get; init; }
    public Currency Currency { get; init; }
    public required Guid MerchantOrderId { get; init; }
    public DateTime MerchantTimestamp { get; init; }
}

internal sealed record PaymentInitResponse
{
    public string? PaymentId { get; init; }
    public string? PaymentUrl { get; init; }
    public string? Message { get; init; }
}
