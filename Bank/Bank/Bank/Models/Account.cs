namespace Bank.Models;

public class Account
{
    public Guid Id { get; set; }
    public required string AccountNumber { get; set; } // Broj žiro računa
    public required string AccountHolderName { get; set; }
    public double Balance { get; set; }

    public List<DebitCard> DebitCards { get; set; } = new();

}
