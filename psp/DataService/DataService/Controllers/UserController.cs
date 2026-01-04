using DataService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        return Ok(new { Message = "Login successful" });
    }
}
