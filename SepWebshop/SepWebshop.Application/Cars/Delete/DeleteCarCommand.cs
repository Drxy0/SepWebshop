using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Cars.Delete;

public sealed record DeleteCarCommand(Guid Id) : ICommand<Guid>;
