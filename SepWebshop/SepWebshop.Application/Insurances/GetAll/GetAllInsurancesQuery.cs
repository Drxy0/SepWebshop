using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Insurances.DTOs;

namespace SepWebshop.Application.Insurances.GetAll;

public sealed record GetAllInsurancesQuery : IQuery<IReadOnlyList<InsuranceDto>>;
