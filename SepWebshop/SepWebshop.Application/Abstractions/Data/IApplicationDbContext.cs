using Microsoft.EntityFrameworkCore;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
