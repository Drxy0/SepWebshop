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

        builder.Property(o => o.OrderStatus)
            .HasConversion(
               os => os.ToString(),
               s => Enum.Parse<OrderStatus>(s));

        builder.Property(o => o.Currency)
            .HasConversion(
                c => c.ToString(),
                s => Enum.Parse<Currency>(s));

        builder.Property(o => o.PaymentMethod)
            .HasConversion(
                pm => pm != null ? pm.ToString() : null,
                s => s != null ? Enum.Parse<PaymentMethodType>(s) : (PaymentMethodType?)null
            )
            .IsRequired(false);
    }
}
