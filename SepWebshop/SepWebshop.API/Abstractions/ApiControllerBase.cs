using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SepWebshop.API.Abstractions;

[Route("api/[controller]")]
[ApiController]
public abstract class ApiControllerBase(ISender mediator) : ControllerBase
{
    protected ISender Mediator { get; } = mediator;
}
