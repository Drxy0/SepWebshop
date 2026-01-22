using Bank.Contracts;
using Bank.Services.Interfaces;
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

    [HttpPost("{paymentRequestId:guid}/pay")]
    public async Task<IActionResult> ExecutePayment(Guid paymentRequestId, [FromBody] PayByCardRequest request)
    {
        var redirectUrl = await _paymentService.ExecuteCardPayment(paymentRequestId, request);

        return Ok(redirectUrl);
    }
}
