using MediatR;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Domain;
using SepWebshop.Domain.Insurances;

namespace SepWebshop.Application.Insurances.Create;

internal sealed class CreateInsuranceCommandHandler(
    IApplicationDbContext context
) : IRequestHandler<CreateInsuranceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        Insurance insurance = new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            PricePerDay = request.PricePerDay,
            DeductibleAmount = request.DeductibleAmount
        };

        try
        {
            context.Insurances.Add(insurance);
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
