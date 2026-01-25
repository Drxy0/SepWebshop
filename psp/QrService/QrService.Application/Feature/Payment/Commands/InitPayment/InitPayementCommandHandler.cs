using MediatR;
using Microsoft.Extensions.Configuration;
using QrService.Domain.Contracts;
using QrService.Domain.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QrService.Application.Feature.Payment.Commands.InitPayment
{
    public class InitPayementCommandHandler :
        IRequestHandler<InitPayementCommandRequest, InitPayementCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public InitPayementCommandHandler(IPaymentRepository paymentRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<InitPayementCommandResponse> Handle(InitPayementCommandRequest request, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("DataServiceClient");
            var response = await client.GetAsync($"d/Payments/{request.MerachanOrderId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Payment details not found in DataService.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var paymentData = await response.Content.ReadFromJsonAsync<DataServicePaymentResponse>(options, cancellationToken);

            if (paymentData == null)
                throw new Exception("Failed to deserialize payment data.");

            var newPayment = new Domain.Entities.Payment
            {
                Id = paymentData.Id,
                MerchantId = paymentData.MerchantId,
                MerchantPassword = paymentData.MerchantPassword,
                Amount = paymentData.Amount,
                Currency = paymentData.Currency,
                MerchantOrderId = paymentData.MerchantOrderId,
                MerchantTimestamp = paymentData.MerchantTimestamp,
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            var isSaved = await _paymentRepository.InitPaymentAsync(newPayment, cancellationToken);

            if (!isSaved)
            {
                throw new Exception("Failed to save payment to QrService database.");
            }

            var baseBankUrl = _configuration["ApiSettings:BankUrl"]?.TrimEnd('/');
            var fullBankUrl = $"{baseBankUrl}/pay/qr/{newPayment.Id}";

            return new InitPayementCommandResponse
            {
                BankUrl = fullBankUrl
            };
        }
    }
}
