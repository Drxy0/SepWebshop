using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Insurances;
using SepWebshop.Domain.Orders;
using SepWebshop.Domain.Users;

namespace SepWebshop.Infrastructure;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Insurance> Insurances { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

}
