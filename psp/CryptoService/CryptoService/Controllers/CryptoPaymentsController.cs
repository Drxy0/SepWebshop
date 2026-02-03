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
        public async Task<IActionResult> InitalizePayment([FromBody] InitializeCryptoPaymentRequest request, CancellationToken cancellationToken)
        {
            InitializeCryptoPaymentResponse? response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);
            if (response is null)
            {
                return BadRequest("Something went wrong");
            }

            return Ok(response);
        }

        [HttpGet("{merchantOrderId:guid}")]
        public async Task<ActionResult<CheckPaymentStatusResponse>> CheckPaymentStatus(Guid merchantOrderId, CancellationToken cancellationToken)
        {
            CheckPaymentStatusResponse? status = await _cryptoPaymentService.CheckPaymentStatusAsync(merchantOrderId, false, cancellationToken);
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

        [HttpGet("{merchantOrderId:guid}/simulate")]
        public async Task<IActionResult> CheckPaymentSimulate(Guid merchantOrderId, CancellationToken cancellationToken)
        {
            CheckPaymentStatusResponse? status = await _cryptoPaymentService.CheckPaymentStatusAsync(merchantOrderId, true, cancellationToken);
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

        [HttpGet("{merchantOrderId:guid}/simulate")]
        public async Task<IActionResult> CheckPaymentStatus_SimulateSuccess(Guid merchantOrderId, CancellationToken cancellationToken)
        {
            CheckPaymentStatusResponse? status = await _cryptoPaymentService.CheckPaymentStatusAsync(merchantOrderId, true, cancellationToken);
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