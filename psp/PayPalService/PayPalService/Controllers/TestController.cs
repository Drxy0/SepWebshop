using Microsoft.AspNetCore.Mvc;

namespace PayPalService.Controllers;

[Route("paypal/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("Pong PayPal");
    }
}
