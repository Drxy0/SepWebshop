using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SepWebshop.Domain.Cars;

namespace SepWebshop.Infrastructure.Cars;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasMany(c => c.Orders)
               .WithOne(o => o.Car)
               .HasForeignKey(o => o.CarId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
