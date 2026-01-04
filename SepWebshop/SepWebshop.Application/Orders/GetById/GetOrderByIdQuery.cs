using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;
