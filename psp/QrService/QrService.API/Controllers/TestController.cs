using Microsoft.AspNetCore.Mvc;

namespace QrService.API.Controllers
{
    [Route("q/test")]
    [ApiController]
    public class TestController : ApiControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong QR");
        }
    }
}
