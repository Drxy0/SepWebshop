using MediatR;
using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Email.SendConfirmRegisterEmail;

public sealed record SendConfirmRegisterEmailCommand(
    string Email,
    string ConfirmationLink
) : ICommand<Unit>;
