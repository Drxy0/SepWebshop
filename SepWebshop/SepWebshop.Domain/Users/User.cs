using SepWebshop.Domain.Orders;

namespace SepWebshop.Domain.Users;

public sealed class User
{
    public Guid Id { get; init; }
    
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string PasswordHash { get; init; }
    public bool IsAdmin { get; set; } = false;

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<OrderDto> Orders { get; set; } = new();
}
