using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Orders.DTOs;

namespace SepWebshop.Application.Orders.GetAllByCarId;

public sealed record GetAllByCarIdQuery(Guid CarId) : IQuery<IReadOnlyList<OrderDto>>;
