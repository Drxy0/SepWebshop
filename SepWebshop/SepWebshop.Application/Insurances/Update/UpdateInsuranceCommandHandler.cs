using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Insurances;

namespace SepWebshop.Application.Insurances.Update;

internal sealed class UpdateInsuranceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<UpdateInsuranceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpdateInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        Insurance? insurance =
            await context.Insurances.FindAsync(request.Id, cancellationToken);

        if (insurance is null)
        {
            return Result.Failure<Guid>(
                InsuranceErrors.NotFound(request.Id));
        }

        insurance.Name = request.Name;
        insurance.Description = request.Description;
        insurance.PricePerDay = request.PricePerDay;
        insurance.DeductibleAmount = request.DeductibleAmount;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(
                Error.Failure("Insurance.DatabaseError", ex.Message));
        }

        return insurance.Id;
    }
}
