using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Users.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password) : ICommand<string>;