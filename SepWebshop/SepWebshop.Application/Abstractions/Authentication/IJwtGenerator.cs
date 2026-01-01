namespace SepWebshop.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    string GenerateAccessToken(Guid userId, string email, bool isAdmin);
    string GenerateRefreshToken();
}
