using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.Login;

internal sealed class LoginUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.InvalidCredentials);
        }

        if (!passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            return Result.Failure<string>(UserErrors.InvalidCredentials);
        }

        string token = ""; // TODO: Generate JWT token

        return token;
    }
}
