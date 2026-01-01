using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Cars.DTOs;

namespace SepWebshop.Application.Cars.GetAll;

public sealed record GetAllCarsQuery : IQuery<IReadOnlyList<CarDto>>;
