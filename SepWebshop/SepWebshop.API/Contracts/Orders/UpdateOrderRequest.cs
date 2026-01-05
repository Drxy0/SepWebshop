using SepWebshop.Domain.Orders;

namespace SepWebshop.API.Contracts.Orders;

public sealed record UpdateOrderRequest(
    Guid OrderId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate
);
