using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Cars.Create;

public sealed record CreateCarCommand(string BrandAndModel, int Year, float Price, string PlateNumber) : ICommand<Guid>;
