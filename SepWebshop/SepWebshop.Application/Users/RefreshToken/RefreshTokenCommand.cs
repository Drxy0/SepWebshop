using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Users.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthResponse>;
