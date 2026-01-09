using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Email.SendConfirmRegisterEmail;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.Register.Commands;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IMediator mediator)
    : IRequestHandler<RegisterUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<string>(UserErrors.EmailNotUnique);
        }

        if (!IsPasswordStrongEnough(command.Password))
        {
            return Result.Failure<string>(UserErrors.WeakPassword);
        }

        using var transaction = await context.BeginTransactionAsync(cancellationToken);

        try
        {
            Guid confirmationToken = Guid.NewGuid();

            User user = new User
            {
                Name = command.Name,
                Surname = command.Surname,
                Email = command.Email,
                PasswordHash = passwordHasher.Hash(command.Password),
                ConfirmationToken = confirmationToken,
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            /*string confirmationLink = GenerateConfirmationLink(user.Id, confirmationToken);
            Result<Unit> emailResult = await mediator.Send(
                new SendConfirmRegisterEmailCommand(user.Email, confirmationLink),
                cancellationToken
            );

            if (emailResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<string>(UserErrors.EmailSendFailed);
            }*/

            await transaction.CommitAsync(cancellationToken);
            return Result.Success("Registration successful, please check your email");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private bool IsPasswordStrongEnough(string password)
    {
        if (password.Length <= 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;

        return true;
    }

    private string GenerateConfirmationLink(Guid userId, Guid token)
    {
        return $"https://localhost:7199/api/Users/confirm-email?userId={userId}&token={token}";
    }
}
