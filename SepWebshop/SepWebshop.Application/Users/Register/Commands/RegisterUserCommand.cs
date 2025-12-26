using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Users.Register.Commands;

public sealed record RegisterUserCommand
    (string Email, string Name, string Surname, string Password) 
    : ICommand<Guid>;
