using Microsoft.EntityFrameworkCore;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Car> Cars { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
