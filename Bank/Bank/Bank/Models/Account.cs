namespace Bank.Models;

public class Account
{
    public Guid Id { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountHolderName { get; set; }
    public decimal Balance { get; set; }

    public List<DebitCard> DebitCards { get; set; } = new();

}
