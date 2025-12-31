using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Authentication;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Users.RefreshToken;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.RefreshTokenl;

internal sealed class RefreshTokenCommandHandler(IApplicationDbContext context, IJwtGenerator jwtGenerator) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Domain.Users.RefreshToken? refreshToken = context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        if (refreshToken is null || refreshToken.ExpiresAtUtc < DateTime.UtcNow || refreshToken.IsRevoked)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        string accessToken = jwtGenerator.GenerateAccessToken(refreshToken.User.Id, refreshToken.User.Email);
        refreshToken.Token = jwtGenerator.GenerateRefreshToken();
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken.Token);
    }
}
