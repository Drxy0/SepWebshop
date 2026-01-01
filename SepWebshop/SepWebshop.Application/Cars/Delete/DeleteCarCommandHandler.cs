using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;

namespace SepWebshop.Application.Cars.Delete;

internal sealed class DeleteCarCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteCarCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        DeleteCarCommand request,
        CancellationToken cancellationToken)
    {
        Car? car = await context.Cars.FindAsync(request.Id, cancellationToken);

        if (car is null)
        {
            return Result.Failure<Guid>(
                CarErrors.NotFound(request.Id));
        }

        try
        {
            context.Cars.Remove(car);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(
                Error.Failure("Car.DatabaseError", ex.Message));
        }

        return car.Id;
    }
}
