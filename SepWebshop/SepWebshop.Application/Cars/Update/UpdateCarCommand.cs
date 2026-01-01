using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Cars.Update;

public sealed record UpdateCarCommand(
    Guid Id,
    string BrandAndModel,
    int Year,
    string PlateNumber) : ICommand<Guid>;
