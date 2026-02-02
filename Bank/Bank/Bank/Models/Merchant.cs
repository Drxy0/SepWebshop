namespace Bank.Models;

public class Merchant
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public Account Account { get; set; } = null!;
    public Guid AccountId { get; set; }
}
