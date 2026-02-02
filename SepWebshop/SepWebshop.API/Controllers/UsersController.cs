using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Abstractions;
using SepWebshop.API.Contracts.Users;
using SepWebshop.Application.Users.ConfirmUser;
using SepWebshop.Application.Users.Login;
using SepWebshop.Application.Users.RefreshToken;
using SepWebshop.Application.Users.Register.Commands;
using SepWebshop.Domain;
namespace SepWebshop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ApiControllerBase
{
    public UsersController(ISender mediator) : base(mediator) { }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Name,
            request.Surname,
            request.Password);

        Result<string> result = await Mediator.Send(command, cToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request, CancellationToken cToken)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password);

        Result<Application.Users.AuthResponse> result = await Mediator.Send(command, cToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cToken)
    {
        var command = new RefreshTokenCommand(request.refreshToken);

        Result<Application.Users.AuthResponse> result = await Mediator.Send(command, cToken);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }
        
        return Ok(result.Value);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] Guid token, CancellationToken cToken)
    {
        var command = new ConfirmEmailCommand(userId, token);

        Result<string> result = await Mediator.Send(command, cToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
