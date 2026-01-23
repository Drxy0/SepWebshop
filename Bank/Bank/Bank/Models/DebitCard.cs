using System.ComponentModel.DataAnnotations;

namespace Bank.Models;

public class DebitCard
{
    public Guid Id { get; set; }

    [MaxLength(19)]
    public required string CardNumber { get; set; }
    public required string CardHolderName { get; set; }
    public required string ExpirationDate { get; set; }
    
    [MaxLength(3)]
    public required string CVV { get; set; }

    public Account Account { get; set; }
    public Guid AccountId { get; set; }
}
