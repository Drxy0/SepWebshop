using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;

namespace SepWebshop.Application.Cars.Update;

internal sealed class UpdateCarCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCarCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        Car? car = await context.Cars.FindAsync(request.Id, cancellationToken);
        
        if (car is null)
        {
            return Result.Failure<Guid>(CarErrors.NotFound(request.Id));
        }

        car.BrandAndModel = request.BrandAndModel;
        car.Year = request.Year;
        car.PlateNumber = request.PlateNumber;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Car.DatabaseError", ex.Message));
        }

        return car.Id;
    }
}
