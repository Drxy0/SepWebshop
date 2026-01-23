namespace Bank.Models;

public class Merchant
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Account Account { get; set; }
    public Guid AccountId { get; set; }
}
