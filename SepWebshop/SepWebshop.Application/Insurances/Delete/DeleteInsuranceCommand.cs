using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Insurances.Delete;

public sealed record DeleteInsuranceCommand(Guid Id) : ICommand<Guid>;
