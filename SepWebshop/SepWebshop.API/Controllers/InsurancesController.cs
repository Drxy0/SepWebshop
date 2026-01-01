using MediatR;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Contracts.Insurances;
using SepWebshop.Application.Insurances.Create;
using SepWebshop.Application.Insurances.Delete;
using SepWebshop.Application.Insurances.GetAll;
using SepWebshop.Application.Insurances.GetById;
using SepWebshop.Application.Insurances.Update;

namespace SepWebshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InsurancesController : ControllerBase
{
    private readonly ISender _sender;

    public InsurancesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInsuranceCommand(
            Name: request.Name,
            Description: request.Description,
            PricePerDay: request.PricePerDay,
            DeductibleAmount: request.DeductibleAmount
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
        var result = await _sender.Send(new GetInsuranceByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllInsurancesQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return Problem(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInsuranceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateInsuranceCommand(
            Id: id,
            Name: request.Name,
            Description: request.Description,
            PricePerDay: request.PricePerDay,
            DeductibleAmount: request.DeductibleAmount
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
        var result = await _sender.Send(new DeleteInsuranceCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return NoContent();
    }
}
