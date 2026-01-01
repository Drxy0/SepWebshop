using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Application.Cars.DTOs;

namespace SepWebshop.Application.Cars.GetById;

public sealed record GetCarByIdQuery(Guid Id)
    : IQuery<CarDto>;
