using SepWebshop.Domain.Orders;

namespace SepWebshop.API.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid CarId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate,
    float TotalPrice,
    PaymentMethodType PaymentMethod);
