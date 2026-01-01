namespace SepWebshop.API.Contracts.Insurances;

public sealed record CreateInsuranceRequest(
    string Name,
    string? Description,
    float PricePerDay,
    float DeductibleAmount
);