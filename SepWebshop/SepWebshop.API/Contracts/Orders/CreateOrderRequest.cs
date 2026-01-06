using SepWebshop.Domain.Orders;

namespace SepWebshop.API.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid CarId,
    Guid InsuranceId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate,
    Currency Currency
);
