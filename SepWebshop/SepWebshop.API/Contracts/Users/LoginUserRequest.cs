namespace SepWebshop.API.Contracts.Users;

public sealed record LoginUserRequest(
    string Email,
    string Password);
