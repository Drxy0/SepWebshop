namespace SepWebshop.Application.Insurances.DTOs;

public sealed record InsuranceDto(
    Guid Id,
    string Name,
    string? Description,
    float PricePerDay,
    float DeductibleAmount
);
