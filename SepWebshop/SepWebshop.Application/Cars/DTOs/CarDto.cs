namespace SepWebshop.Application.Cars.DTOs;

public sealed record CarDto(
    Guid Id,
    string BrandAndModel,
    int Year,
    float Price,
    string PlateNumber
);
