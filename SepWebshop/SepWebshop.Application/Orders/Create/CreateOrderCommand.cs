using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Create;

public sealed record CreateOrderCommand(
    Guid UserId,
    Guid CarId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate,
    float TotalPrice,
    PaymentMethodType PaymentMethod
) : ICommand<Guid>;
