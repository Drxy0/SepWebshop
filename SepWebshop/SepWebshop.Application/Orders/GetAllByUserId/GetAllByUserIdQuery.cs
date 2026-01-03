using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.GetAllByUserId;

public sealed record GetAllByUserIdQuery(Guid UserId) : IQuery<IReadOnlyList<Order>>;
