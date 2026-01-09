using DataService.Contracts;
using DataService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataService.Controllers;

[ApiController]
[Route("d/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.Login(request.Username, request.Password);
        if (!result.isSuccess)
        {
            return Unauthorized();
        }
        return Ok(new { AccessToken = result.accessToken });
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var success = await _userService.Register(request);
        if (!success)
            return BadRequest(new { Message = "Username or MerchantId already taken." });

        return Ok(new { Message = "Registration successful" });
    }
}
