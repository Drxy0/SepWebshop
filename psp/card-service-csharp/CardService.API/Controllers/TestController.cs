using Microsoft.AspNetCore.Mvc;

namespace CardService.API.Controllers
{
    [Route("ca/test")]
    [ApiController]
    public class TestController : ApiControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong Card");
        }
    }
}
