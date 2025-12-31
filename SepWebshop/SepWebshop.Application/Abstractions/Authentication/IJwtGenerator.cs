namespace SepWebshop.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
}
