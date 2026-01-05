using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Orders.Delete;

public sealed record DeleteOrderCommand(Guid OrderId) : ICommand<Guid>;
