using SepWebshop.Application.Orders.DTOs;
using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Orders.GetAllByUserId;

public sealed record GetAllByUserIdQuery(Guid UserId) : IQuery<IReadOnlyList<OrderDto>>;
