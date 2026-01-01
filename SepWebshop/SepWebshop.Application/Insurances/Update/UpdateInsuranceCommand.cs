using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Insurances.Update;

public sealed record UpdateInsuranceCommand(
    Guid Id,
    string Name,
    string? Description,
    float PricePerDay,
    float DeductibleAmount
) : ICommand<Guid>;
