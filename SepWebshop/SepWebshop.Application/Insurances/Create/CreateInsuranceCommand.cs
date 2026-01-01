using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Insurances.Create;

public sealed record CreateInsuranceCommand(
    string Name,
    string? Description,
    float PricePerDay,
    float DeductibleAmount
) : ICommand<Guid>;
