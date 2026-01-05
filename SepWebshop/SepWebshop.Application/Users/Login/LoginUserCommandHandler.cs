using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.Login;

internal sealed class LoginUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtGenerator jwtGenerator) 
    : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        if (!user.IsAccountActive)
        {
            return Result.Failure<AuthResponse>(UserErrors.EmailNotConfirmed);
        }

        if (!passwordHasher.Verify(user.PasswordHash, command.Password))
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        string accessToken = jwtGenerator.GenerateAccessToken(user.Id, user.Email, user.IsAdmin);
        string refreshToken = jwtGenerator.GenerateRefreshToken();

        await context.RefreshTokens.AddAsync(new Domain.Users.RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken);
    }
}
