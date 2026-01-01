using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Cars.Create;

public sealed record CreateCarCommand(string BrandAndModel, int Year, string PlateNumber) : ICommand<Guid>;
