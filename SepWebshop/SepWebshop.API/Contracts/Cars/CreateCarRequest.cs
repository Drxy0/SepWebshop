namespace SepWebshop.API.Contracts.Cars;

public sealed record CreateCarRequest(string BrandAndModel, int Year, string PlateNumber, float Price);
