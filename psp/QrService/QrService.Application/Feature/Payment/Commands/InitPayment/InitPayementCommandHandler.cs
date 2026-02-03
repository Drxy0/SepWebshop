using MediatR;
using Microsoft.Extensions.Configuration;
using QrService.Application.Abstractions.Bank;
using QrService.Domain.Contracts;
using QrService.Domain.DTOs;
using QrService.Domain.DTOs.Bank;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QrService.Application.Feature.Payment.Commands.InitPayment
{
    public class InitPayementCommandHandler : IRequestHandler<InitPayementCommandRequest, InitPayementCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IBankClient _bankClient;
        
        public InitPayementCommandHandler(IPaymentRepository paymentRepository, IHttpClientFactory httpClientFactory, 
            IBankClient bankClient, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _bankClient = bankClient;
        }

        public async Task<InitPayementCommandResponse> Handle(InitPayementCommandRequest request, CancellationToken cancellationToken)
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

            var payment = new Domain.Entities.Payment
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

            var existingPayment = await _paymentRepository.GetByIdAsync(payment.Id, cancellationToken);

            bool isSaved;

            if (existingPayment != null)
            {
                existingPayment.MerchantId = payment.MerchantId;
                existingPayment.MerchantPassword = payment.MerchantPassword;
                existingPayment.Amount = payment.Amount;
                existingPayment.Currency = payment.Currency;
                existingPayment.MerchantOrderId = payment.MerchantOrderId;
                existingPayment.MerchantTimestamp = payment.MerchantTimestamp;
                existingPayment.IsProcessed = payment.IsProcessed;

                isSaved = await _paymentRepository.UpdateAsync(existingPayment, cancellationToken);
            }
            else
            {
                isSaved = await _paymentRepository.InitPaymentAsync(payment, cancellationToken);
            }

            if (!isSaved)
            {
                throw new Exception("Failed to persist payment in QrService database.");
            }

            var bankRequest = new BankInitPaymentRequest(
                MerchantId: paymentData.MerchantId,
                PspPaymentId: paymentData.Id,
                Amount: paymentData.Amount,
                Currency: paymentData.Currency,
                Stan: paymentData.MerchantOrderId.ToString(),
                PspTimestamp: paymentData.MerchantTimestamp
            );

            BankInitPaymentResponse bankResponse =
                await _bankClient.InitQrPaymentAsync(bankRequest, cancellationToken);

            return new InitPayementCommandResponse
            {
                BankUrl = bankResponse.PaymentUrl
            };
        }

    }
}
