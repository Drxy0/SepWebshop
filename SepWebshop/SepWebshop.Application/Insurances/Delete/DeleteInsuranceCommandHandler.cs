using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Insurances;

namespace SepWebshop.Application.Insurances.Delete;

internal sealed class DeleteInsuranceCommandHandler(
    IApplicationDbContext context) : IRequestHandler<DeleteInsuranceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        DeleteInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        Insurance? insurance =
            await context.Insurances.FindAsync(request.Id, cancellationToken);

        if (insurance is null)
        {
            return Result.Failure<Guid>(
                InsuranceErrors.NotFound(request.Id));
        }

        try
        {
            context.Insurances.Remove(insurance);
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
