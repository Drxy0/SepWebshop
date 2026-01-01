using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Contracts.Cars;
using SepWebshop.Application.Cars.Create;
using SepWebshop.Application.Cars.Delete;
using SepWebshop.Application.Cars.GetAll;
using SepWebshop.Application.Cars.GetById;
using SepWebshop.Application.Cars.Update;

namespace SepWebshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CarsController : ControllerBase
{
    private readonly ISender _sender;

    public CarsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCarCommand(
            BrandAndModel: request.BrandAndModel,
            Year: request.Year,
            PlateNumber: request.PlateNumber
        );

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Problem(result.Error.Description);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCarByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllCarsQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return Problem(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCarRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCarCommand(
            Id: id,
            BrandAndModel: request.BrandAndModel,
            Year: request.Year,
            PlateNumber: request.PlateNumber
        );

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCarCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return NoContent();
    }
}
