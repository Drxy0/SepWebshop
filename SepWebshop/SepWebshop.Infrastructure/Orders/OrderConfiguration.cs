using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepWebshop.Domain.Orders;

namespace SepWebshop.Infrastructure.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasOne(o => o.User)
               .WithMany()
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Car)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CarId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Insurance)
           .WithMany()
           .HasForeignKey(o => o.InsuranceId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.CarId);
        builder.HasIndex(o => o.InsuranceId);
    }
}
