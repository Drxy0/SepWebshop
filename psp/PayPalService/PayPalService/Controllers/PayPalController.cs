using Microsoft.AspNetCore.Mvc;
using PayPalService.Services;

namespace PayPalService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PayPalController : ControllerBase
{
    private readonly PayPalGatewayService _service;

    public PayPalController(PayPalGatewayService service)
    {
        _service = service;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromQuery] decimal amount, [FromQuery] string currency)
    {
        string redirectUrl = await _service.CreateOrderAsync(amount, currency);

        return Ok(new { url = redirectUrl });
    }

    [HttpGet("return")]
    public async Task<IActionResult> Return([FromQuery(Name = "token")] string orderId)
    {
        await _service.CaptureAsync(orderId);

        return Ok("Payment successful");
    }

    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        return Ok("Payment cancelled");
    }
}
