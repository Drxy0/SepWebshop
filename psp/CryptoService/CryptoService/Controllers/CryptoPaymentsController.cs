using CryptoService.DTOs;
using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers
{
    [Route("crypto/Payments")]
    [ApiController]
    public class CryptoPaymentsController : ControllerBase
    {
        private readonly ICryptoPaymentService _cryptoPaymentService;

        public CryptoPaymentsController(ICryptoPaymentService cryptoPaymentService)
        {
            _cryptoPaymentService = cryptoPaymentService;
        }

        [HttpPost("init")]
        public async Task<ActionResult<InitializeCryptoPaymentResponse>> InitalizePayment([FromBody] InitializeCryptoPaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
            {
                return BadRequest("Fiat amount must be greater than zero.");
            }

            InitializeCryptoPaymentResponse? response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);

            if (response is null)
            {
                return BadRequest("Something went wrong");
            }

            return Ok(response);
        }

        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<CheckCryptoPaymentStatusResponse>> CheckPaymentStatus(Guid paymentId, CancellationToken cancellationToken)
        {
            CheckCryptoPaymentStatusResponse? status = await _cryptoPaymentService.CheckPaymentStatusAsync(paymentId, false, cancellationToken);
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

        [HttpGet("{paymentId:guid}/qr-code")]
        public async Task<IActionResult> GetPaymentQrCode(Guid paymentId, CancellationToken cancellationToken)
        {
            try
            {
                byte[] qrCode = await _cryptoPaymentService.GeneratePaymentQrCodeAsync(paymentId, cancellationToken);
                return File(qrCode, "image/png");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}