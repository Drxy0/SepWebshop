using Bank.Contracts;
using Bank.Contracts.QR;
using Bank.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Bank.Controllers;

[ApiController]
[Route("api/bank/[controller]")]
public class PaymentsController : ControllerBase
{
    public IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("init")]
    [DisableCors]
    public async Task<IActionResult> InitPayment(
        [FromBody] PaymentInitRequest dto,
        [FromHeader(Name = "PspID")] Guid pspId,
        [FromHeader(Name = "Signature")] string signature,
        [FromHeader(Name = "Timestamp")] DateTime timestamp,
        [FromHeader(Name = "IsQrPayment")] bool isQrPayment)
    {
        if (Math.Abs((DateTime.UtcNow - timestamp).TotalMinutes) > 30)
            return Unauthorized("Timestamp expired");

        InitializePaymentServiceResult result = await _paymentService.InitializePayment(dto, pspId, signature, timestamp, isQrPayment);

        return result.Result switch
        {
            InitializePaymentResult.Success =>
                Ok(result.Response),

            InitializePaymentResult.InvalidPsp =>
                Unauthorized("Invalid PSP"),

            InitializePaymentResult.InvalidSignature =>
                Unauthorized("Invalid signature"),

            InitializePaymentResult.InvalidMerchant =>
                BadRequest("Invalid merchant"),

            _ => StatusCode(500)
        };
    }


    [HttpGet("{paymentRequestId:guid}")]
    public async Task<IActionResult> GetPaymentRequest(Guid paymentRequestId)
    {
        var paymentRequest = await _paymentService.GetPaymentRequest(paymentRequestId);

        return Ok(paymentRequest);
    }

    [HttpPost("card/{paymentRequestId:guid}")]
    public async Task<IActionResult> ExecutePayment(Guid paymentRequestId, [FromBody] PayByCardRequest request)
    {
        var redirectUrl = await _paymentService.ExecuteCardPayment(paymentRequestId, request);

        return Ok(redirectUrl);
    }


    [HttpPost("qr/{paymentRequestId}")]
    public async Task<ActionResult<QRPaymentResponseDto>> GenerateQrPayment(Guid paymentRequestId)
    {
        var result = await _paymentService.GenerateQrPayment(paymentRequestId);
        return Ok(result);
    }

    [HttpPost("qr/{paymentRequestId}/process")]
    public async Task<ActionResult<QRPaymentResponseDto>> ProcessQrPayment(Guid paymentRequestId, [FromBody] ProcessQrPaymentRequest request)
    {
        try
        {
            var result = await _paymentService.ProcessQrPayment(
                paymentRequestId,
                request.CustomerAccountNumber);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // TODO: ovo bi trebalo da simulira pravi IPS, vjerovatno treba obrisati 
    [HttpPost("qr/ips-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleIpsCallback([FromBody] IpsCallbackDto callbackData)
    {
        try
        {
            // TODO: Add signature validation here
            // string expectedSignature = CalculateSignature(callbackData);
            // if (callbackData.Signature != expectedSignature) return Unauthorized();

            await _paymentService.ProcessIpsCallback(callbackData);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("qr/{paymentRequestId:guid}/status")]
    public async Task<IActionResult> GetQrPaymentStatus(Guid paymentRequestId)
    {
        try
        {
            var status = await _paymentService.GetQrPaymentStatus(paymentRequestId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
