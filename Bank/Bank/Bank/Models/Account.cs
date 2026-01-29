using System.ComponentModel.DataAnnotations;

namespace Bank.Models;

public class Account
{
    public Guid Id { get; set; }

    [Length(18,18)]
    public required string AccountNumber { get; set; }
    public required string AccountHolderName { get; set; }
    public double Balance { get; set; }

    public AccountType Type { get; set; } = AccountType.Customer;

    public List<DebitCard> DebitCards { get; set; } = new();
}
