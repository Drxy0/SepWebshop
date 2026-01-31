using CryptoService.Clients;
using CryptoService.Clients.Interfaces;
using CryptoService.DTOs;
using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers
{
    [Route("crypto/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ICryptoPaymentService _cryptoPaymentService;
        private readonly IWebshopClient _webshopClient;

        public TestController(ICryptoPaymentService cryptoPaymentService, IWebshopClient webshopClient)
        {
            _cryptoPaymentService = cryptoPaymentService;
            _webshopClient = webshopClient;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("CryptoService is alive");
        }

        [HttpPost("setup/generate-shop-wallet")]
        public async Task<ActionResult> GenerateShopWallet()
        {
            var (wif, address) = await _cryptoPaymentService.GenerateShopWalletAsync();

            return Ok(new
            {
                WIF = wif,
                Address = address,
                Instructions = "1. Copy WIF to appsettings.json, 2. You don't need to fund this address (customers pay TO it)"
            });
        }

        [HttpPost("update-webshop/{orderId:guid}")]
        public async Task<IActionResult> UpdateWebshop(Guid orderId)
        {
            bool responseSuccess = await _webshopClient.SendAsync(orderId, true);

            if (!responseSuccess)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet("{paymentId:guid}/simulate")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> CheckPaymentSimulate(Guid paymentId, CancellationToken cancellationToken)
        {
            CryptoPaymentStatusResponse? status = await _cryptoPaymentService.CheckPaymentStatusAsync(paymentId, true, cancellationToken);
            if (status is null)
            {
                return NotFound();
            }

            if (!status.WebshopNotified)
            {
                return Problem("Payment was succesful, but we failed to notify the webshop.", statusCode: 500);
            }

            return Ok(status);
        }
    }
}
