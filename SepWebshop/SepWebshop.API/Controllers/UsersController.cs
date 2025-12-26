using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Contracts.Users;
using SepWebshop.Application.Users.Register.Commands;
using SepWebshop.Domain;
namespace SepWebshop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Name,
            request.Surname,
            request.Password);

        Result<Guid> result = await _sender.Send(command, cToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);

    }
}
