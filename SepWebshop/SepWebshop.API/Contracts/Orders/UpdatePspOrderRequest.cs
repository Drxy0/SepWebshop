using SepWebshop.Domain.Orders;

namespace SepWebshop.API.Contracts.Orders;

public sealed record UpdatePspOrderRequest
(
    Guid OrderId,
    OrderStatus OrderStatus,
    PaymentMethodType PaymentMethod,
    string PspId,
    string PspPassword
);
