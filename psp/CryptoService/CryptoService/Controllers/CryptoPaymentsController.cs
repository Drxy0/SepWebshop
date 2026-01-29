using CryptoService.DTOs;
<<<<<<< HEAD
using CryptoService.Services;
=======
>>>>>>> 69563e27af42f2b9bb3170db53e264e459d0c617
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

        /// <summary>
        /// Create a new crypto payment (invoice)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateCryptoPaymentResponse>> CreatePayment([FromBody] CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
            {
                return BadRequest("Fiat amount must be greater than zero.");
            }

            CreateCryptoPaymentResponse response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Get the status of a crypto payment
        /// </summary>
        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> GetPaymentStatus(Guid paymentId, CancellationToken cancellationToken)
        {
            CryptoPaymentStatusResponse? status = await _cryptoPaymentService.GetStatusAsync(paymentId, cancellationToken);

            if (status is null)
            {
                return NotFound();
            }

            return Ok(status);
        }
<<<<<<< HEAD


        [HttpPost("{paymentId:guid}/process")]
        public async Task<ActionResult<string>> ProcessPayment(Guid paymentId, CancellationToken cancellationToken)
        {
            try
            {
                string txId = await _cryptoPaymentService.ProcessPaymentAsync(paymentId, cancellationToken);
                return Ok(new { TransactionId = txId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
=======
>>>>>>> 69563e27af42f2b9bb3170db53e264e459d0c617
    }
}
