using SepWebshop.Domain.Cars;
using SepWebshop.Domain.Users;

namespace SepWebshop.Domain.Reservations;

public sealed class Reservation
{
    public Guid Id { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid CarId { get; set; }
    public Car Car { get; set; } = null!;
}
