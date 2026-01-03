using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SepWebshop.API.Contracts.Orders;
using SepWebshop.Application.Orders.Create;
using SepWebshop.Application.Orders.Delete;
using SepWebshop.Application.Orders.GetAllByUserId;
using SepWebshop.Application.Orders.GetById;
using SepWebshop.Application.Orders.Update;

namespace SepWebshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.UserId,
            request.CarId,
            request.LeaseStartDate,
            request.LeaseEndDate,
            request.TotalPrice,
            request.PaymentMethod);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { orderId = result.Value }, result.Value);
    }

    [Authorize]
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(orderId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllByUserIdQuery(userId), cancellationToken);
        return Ok(result.Value);
    }

    [Authorize]
    [HttpPut("{orderId:guid}")]
    public async Task<IActionResult> Update(
        Guid orderId,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (orderId != request.OrderId)
        {
            return BadRequest("OrderId mismatch");
        }

        var command = new UpdateOrderCommand(
            request.OrderId,
            request.LeaseStartDate,
            request.LeaseEndDate,
            request.TotalPrice,
            request.PaymentMethod);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpDelete("{orderId:guid}")]
    public async Task<IActionResult> Delete(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteOrderCommand(orderId), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return NoContent();
    }
}
