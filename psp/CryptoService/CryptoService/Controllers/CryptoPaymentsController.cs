using CryptoService.DTOs;
<<<<<<< HEAD
<<<<<<< HEAD
using CryptoService.Services;
<<<<<<< HEAD
=======
>>>>>>> 5ab45fd (Finish crypto backend)
using CryptoService.Services.Interfaces;
=======
>>>>>>> 5cbd7fe (Add base implementation)
=======
using CryptoService.Services.Interfaces;
>>>>>>> 69563e2 (Add wallet, start)
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

        [HttpPost]
        public async Task<ActionResult<CreateCryptoPaymentResponse>> CreatePayment([FromBody] CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
            {
                return BadRequest("Fiat amount must be greater than zero.");
            }

            var response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> GetPaymentStatus(Guid paymentId, CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.GetStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }

        [HttpPost("{paymentId:guid}/check")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> CheckPayment(Guid paymentId, CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.CheckPaymentStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 5cbd7fe (Add base implementation)
=======
>>>>>>> 5ab45fd (Finish crypto backend)
=======

        [HttpGet("{paymentId:guid}/qrcode")]
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
>>>>>>> b85b831 (Add qr code endpoint)
    }
}