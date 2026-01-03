using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Cars.Create;

internal sealed class CreateCarCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCarCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        Car car = new Car
        {
            Id = Guid.NewGuid(),
            BrandAndModel = request.BrandAndModel,
            Year = request.Year,
            Price = request.Price,
            PlateNumber = request.PlateNumber
        };

        try
        {
            context.Cars.Add(car);

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Car.DatabaseError", ex.Message));
        }

        return car.Id;
    }
}
