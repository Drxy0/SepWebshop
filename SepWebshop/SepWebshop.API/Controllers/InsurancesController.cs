using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Abstractions;
using SepWebshop.API.Contracts.Insurances;
using SepWebshop.Application.Insurances.Create;
using SepWebshop.Application.Insurances.Delete;
using SepWebshop.Application.Insurances.GetAll;
using SepWebshop.Application.Insurances.GetById;
using SepWebshop.Application.Insurances.Update;

namespace SepWebshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InsurancesController : ApiControllerBase
{
    public InsurancesController(ISender mediator) : base(mediator) { }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateInsuranceCommand(
            Name: request.Name,
            Description: request.Description,
            PricePerDay: request.PricePerDay,
            DeductibleAmount: request.DeductibleAmount
        );

        var result = await  Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Problem(result.Error.Description);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await  Mediator.Send(new GetInsuranceByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await  Mediator.Send(new GetAllInsurancesQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return Problem(result.Error.Description);
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
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

        var result = await  Mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await  Mediator.Send(new DeleteInsuranceCommand(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error.Description);
        }

        return NoContent();
    }
}
