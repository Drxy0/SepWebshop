using DataService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataService.Persistance;

public class DataServiceDbContext : DbContext
{
    public DataServiceDbContext(DbContextOptions<DataServiceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.PaymentMethods)
            .WithMany()
            .UsingEntity(j => j.ToTable("UserPaymentMethods"));
    }
}
