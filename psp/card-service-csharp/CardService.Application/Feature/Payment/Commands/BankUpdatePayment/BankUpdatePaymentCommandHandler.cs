using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using CardService.Domain.Contracts;
using CardService.Domain.Settings;
using System.Net.Http.Json;

namespace CardService.Application.Feature.Payment.Commands.BankUpdatePayment
{
    public class BankUpdatePaymentCommandHandler : IRequestHandler<BankUpdatePaymentCommandRequest, BankUpdatePaymentCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PspSettings _pspSettings;
        private readonly IConfiguration _configuration;

        public BankUpdatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<PspSettings> pspOptions,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _httpClientFactory = httpClientFactory;
            _pspSettings = pspOptions.Value;
            _configuration = configuration;
        }
        
        public async Task<BankUpdatePaymentCommandResponse> Handle(BankUpdatePaymentCommandRequest request, CancellationToken cancellationToken)
        {
            var bankId = _configuration["Bank:BankId"];
            var bankPass = _configuration["Bank:BankPassword"];

            if (request.BankId != bankId || request.BankPassword != bankPass)
            {
                throw new Exception("Unauthorized Bank access");
            }

            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null) throw new Exception("Payment not found");

            payment.IsProcessed = true;
            await _paymentRepository.UpdateAsync(payment, cancellationToken);

            var client = _httpClientFactory.CreateClient("WebShopClient");

            var webShopRequest = new
            {
                OrderId = payment.MerchantOrderId,
                OrderStatus = "Completed",
                PaymentMethod = "Card",
                PspId = _pspSettings.PspId,
                PspPassword = _pspSettings.PspPassword
            };

            var response = await client.PutAsJsonAsync($"api/Orders/psp/{payment.MerchantOrderId}", webShopRequest, cancellationToken);

            return new BankUpdatePaymentCommandResponse
            {
                Status = response.IsSuccessStatusCode ? "Success" : "WebShop Update Failed"
            };
        }
    }
}
