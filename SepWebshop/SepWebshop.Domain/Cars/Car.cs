namespace SepWebshop.Domain.Cars;

public sealed class Car
{
    public Guid Id { get; init; }
    public string BrandAndModel { get; set; } = null!;
    public int Year { get; set; }
    public float Price { get; set; }
    public string PlateNumber { get; set; } = null!;
}
