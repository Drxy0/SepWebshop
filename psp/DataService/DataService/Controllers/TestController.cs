using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers
{
    [Route("d/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong DataService");
        }
    }
}
