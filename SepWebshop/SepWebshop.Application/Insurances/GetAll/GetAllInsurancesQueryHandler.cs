using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Insurances.DTOs;
using SepWebshop.Domain;

namespace SepWebshop.Application.Insurances.GetAll;

internal sealed class GetAllInsurancesQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetAllInsurancesQuery, Result<IReadOnlyList<InsuranceDto>>>
{
    public async Task<Result<IReadOnlyList<InsuranceDto>>> Handle(
        GetAllInsurancesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<InsuranceDto> insurances = await context.Insurances
            .Select(i => new InsuranceDto(
                i.Id,
                i.Name,
                i.Description,
                i.PricePerDay,
                i.DeductibleAmount))
            .ToListAsync(cancellationToken);

        return Result.Success(insurances);
    }
}
