namespace SepWebshop.Domain.Cars;

public static class CarErrors
{
    public static Error NotFound(Guid carId) => Error.NotFound(
        "Cars.NotFound",
        $"The car with the Id = '{carId}' was not found");
}
