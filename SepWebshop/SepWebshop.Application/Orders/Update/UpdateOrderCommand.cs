using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Update;

public sealed record UpdateOrderCommand(
    Guid OrderId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate,
    float TotalPrice,
    bool IsCompleted,
    PaymentMethodType PaymentMethod
) : ICommand<Guid>;
