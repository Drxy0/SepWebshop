namespace SepWebshop.Domain.Insurances;

public sealed class Insurance
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public float PricePerDay { get; set; }
    public float DeductibleAmount { get; set; }
}
