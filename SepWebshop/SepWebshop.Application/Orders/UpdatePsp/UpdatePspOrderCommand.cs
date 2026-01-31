using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.UpdatePsp;
public sealed record UpdatePspOrderCommand(
    Guid OrderId,
    OrderStatus OrderStatus,
    PaymentMethodType PaymentMethod,
    string PspId,
    string PspPassword
) : ICommand<Guid>;

