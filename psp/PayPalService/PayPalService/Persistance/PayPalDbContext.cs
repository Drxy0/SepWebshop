using Microsoft.EntityFrameworkCore;
using PayPalService.Models;

namespace PayPalService.Persistance;

public class PayPalDbContext : DbContext
{
    public PayPalDbContext(DbContextOptions<PayPalDbContext> options) : base(options) { }

    public DbSet<PayPalPayment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
