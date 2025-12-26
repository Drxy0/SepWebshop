namespace SepWebshop.API.Contracts.Users;

public sealed record RegisterUserRequest(
    string Email,
    string Name,
    string Surname,
    string Password);
