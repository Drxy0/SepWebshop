namespace SepWebshop.Application.Users;

public sealed record AuthResponse(string accessToken, string refreshToken);
