using Microsoft.AspNetCore.Mvc;
using PayPalService.Clients;
using PayPalService.DTOs;
using PayPalService.Services.Interfaces;

namespace PayPalService.Controllers;

[Route("paypal/Payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly PayPalGatewayService _service;
    private readonly IPaymentService _paymentService;

    public PaymentsController(PayPalGatewayService service, IPaymentService paymentService)
    {
        _service = service;
        _paymentService = paymentService;
    }

    [HttpPost("init")]
    public async Task<IActionResult> InitalizePayment([FromBody] InitializePaymentRequest request, CancellationToken cancellationToken)
    {
        InitializePaymentResponse? response = await _paymentService.CreatePaymentAsync(request, cancellationToken);
        if (response is null)
        {
            return BadRequest("Something went wrong");
        }

        return Ok(response);
    }

    [HttpGet("return")]
    public async Task<IActionResult> Return([FromQuery(Name = "token")] string orderId)
    {
        var response = await _service.CaptureAsync(orderId);

        if (response.success == false)
        {
            return BadRequest("Payment failed");
        }

        return Redirect(response.redirectUrl);
    }

    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        return Ok("Payment cancelled");
    }
}
