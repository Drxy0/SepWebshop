using SepWebshop.Application.Abstractions.Messaging;

namespace SepWebshop.Application.Cars.Update;

public sealed record UpdateCarCommand(
    Guid Id,
    string BrandAndModel,
    int Year,
    float Price,
    string PlateNumber) : ICommand<Guid>;
