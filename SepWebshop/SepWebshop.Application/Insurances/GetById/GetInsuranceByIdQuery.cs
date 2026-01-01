using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Insurances.DTOs;

namespace SepWebshop.Application.Insurances.GetById;

public sealed record GetInsuranceByIdQuery(Guid Id) : IQuery<InsuranceDto>;
