using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.Register.Commands;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        if (!IsPasswordStrongEnough(command.Password))
        {
            return Result.Failure<Guid>(UserErrors.WeakPassword);
        }

        User user = new User
        {
            Name = command.Name,
            Surname = command.Surname,
            Email = command.Email,
            PasswordHash = passwordHasher.Hash(command.Password),
        };

        context.Users.Add(user);

        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
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
}
