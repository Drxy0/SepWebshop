using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetAllByCarId;

public sealed record GetAllByCarIdQuery(Guid CarId) : IQuery<IReadOnlyList<OrderDto>>;
