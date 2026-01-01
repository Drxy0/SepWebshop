using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Insurances.DTOs;
using SepWebshop.Domain;
using SepWebshop.Domain.Insurances;

namespace SepWebshop.Application.Insurances.GetById;

internal sealed class GetInsuranceByIdQueryHandler(
    IApplicationDbContext context) : IRequestHandler<GetInsuranceByIdQuery, Result<InsuranceDto>>
{
    public async Task<Result<InsuranceDto>> Handle(
        GetInsuranceByIdQuery request,
        CancellationToken cancellationToken)
    {
        InsuranceDto? insurance = await context.Insurances
            .Where(i => i.Id == request.Id)
            .Select(i => new InsuranceDto(
                i.Id,
                i.Name,
                i.Description,
                i.PricePerDay,
                i.DeductibleAmount))
            .FirstOrDefaultAsync(cancellationToken);

        if (insurance is null)
        {
            return Result.Failure<InsuranceDto>(
                InsuranceErrors.NotFound(request.Id));
        }

        return insurance;
    }
}
