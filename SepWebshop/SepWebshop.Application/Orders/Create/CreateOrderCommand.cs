using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Application.Orders.Create;

public sealed record CreateOrderCommand(
    Guid UserId,
    Guid CarId,
    Guid InsuranceId,
    DateTime LeaseStartDate,
    DateTime LeaseEndDate
) : ICommand<Guid>;
