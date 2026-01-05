using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Orders.Update;

public sealed record UpdateOrderCommand(
    Guid OrderId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate
) : ICommand<Guid>;
