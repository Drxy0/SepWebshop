using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Users.ConfirmUser;

public sealed record ConfirmEmailCommand(Guid UserId, Guid Token) : ICommand<string>;
