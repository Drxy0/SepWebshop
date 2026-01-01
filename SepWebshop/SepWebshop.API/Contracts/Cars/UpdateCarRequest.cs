namespace SepWebshop.API.Contracts.Cars;

public sealed record UpdateCarRequest(string BrandAndModel, int Year, string PlateNumber, float Price);

