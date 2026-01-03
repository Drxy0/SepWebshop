using Microsoft.EntityFrameworkCore;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Insurances;
using SepWebshop.Domain.Orders;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Car> Cars { get; }
    DbSet<Insurance> Insurances { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
