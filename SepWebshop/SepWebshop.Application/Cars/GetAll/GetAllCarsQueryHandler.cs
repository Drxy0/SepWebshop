using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Cars.DTOs;
using SepWebshop.Domain;

namespace SepWebshop.Application.Cars.GetAll;

internal sealed class GetAllCarsQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAllCarsQuery, Result<IReadOnlyList<CarDto>>>
{
    public async Task<Result<IReadOnlyList<CarDto>>> Handle(
        GetAllCarsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CarDto> cars = await context.Cars
            .Select(c => new CarDto(
                c.Id,
                c.BrandAndModel,
                c.Year,
                c.Price,
                c.PlateNumber))
            .ToListAsync(cancellationToken);

        return Result.Success(cars);
    }
}
