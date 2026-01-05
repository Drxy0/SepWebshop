namespace SepWebshop.Domain.Users;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }

    public User User { get; set; }

    public bool IsValid() =>
        !IsRevoked && ExpiresAtUtc > DateTime.UtcNow;
}
