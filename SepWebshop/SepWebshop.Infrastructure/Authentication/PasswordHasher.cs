using SepWebshop.Application.Abstractions.Authentication;

namespace SepWebshop.Infrastructure.Authentication;

internal sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) 
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string hashedPassword, string providedPassword) 
        => BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
}
