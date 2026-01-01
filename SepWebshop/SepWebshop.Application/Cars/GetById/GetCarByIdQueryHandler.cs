using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Cars.DTOs;
using SepWebshop.Domain;
using SepWebshop.Domain.Cars;

namespace SepWebshop.Application.Cars.GetById;

internal sealed class GetCarByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetCarByIdQuery, Result<CarDto>>
{
    public async Task<Result<CarDto>> Handle(
        GetCarByIdQuery request,
        CancellationToken cancellationToken)
    {
        CarDto? car = await context.Cars
            .Where(c => c.Id == request.Id)
            .Select(c => new CarDto(
                c.Id,
                c.BrandAndModel,
                c.Year,
                c.Price,
                c.PlateNumber))
            .FirstOrDefaultAsync(cancellationToken);

        if (car is null)
        {
            return Result.Failure<CarDto>(
                CarErrors.NotFound(request.Id));
        }

        return Result.Success(car);
    }
}
