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

        /// <summary>
        /// Create a new crypto payment invoice
        /// Returns BTC address and amount for customer to pay
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateCryptoPaymentResponse>> CreatePayment(
            [FromBody] CreateCryptoPaymentRequest request,
            CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
                return BadRequest("Fiat amount must be greater than zero.");

            var response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get current status of a payment (from database only)
        /// </summary>
        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> GetPaymentStatus(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.GetStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }

        /// <summary>
        /// Check blockchain for payment confirmation
        /// Call this after customer sends BTC from their wallet
        /// </summary>
        [HttpPost("{paymentId:guid}/check")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> CheckPayment(
            Guid paymentId,
            CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.CheckPaymentStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }
    }
}