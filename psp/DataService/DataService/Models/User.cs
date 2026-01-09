namespace DataService.Models;

public class User
{
    public Guid Id { get; init; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public string MearchantId { get; set; } = string.Empty;
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}
