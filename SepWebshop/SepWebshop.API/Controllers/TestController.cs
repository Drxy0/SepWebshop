using Microsoft.AspNetCore.Mvc;

namespace SepWebshop.API.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet()]
        public IActionResult Test()
        {
            return Ok("Test successful!");
        }
    }
}
