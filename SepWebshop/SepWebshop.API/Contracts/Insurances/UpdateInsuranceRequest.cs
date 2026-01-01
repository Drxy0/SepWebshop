namespace SepWebshop.API.Contracts.Insurances;

public sealed record UpdateInsuranceRequest(
    string Name,
    string? Description,
    float PricePerDay,
    float DeductibleAmount
);
