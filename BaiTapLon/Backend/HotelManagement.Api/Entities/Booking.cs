namespace HotelManagement.Api.Entities;

public enum BookingStatus
{
    PENDING,
    CONFIRMED,
    CHECKED_IN,
    CHECKED_OUT,
    CANCELLED
}

public class Booking
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public string BookingCode { get; set; } = null!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PENDING;
    public DateTime CreatedAt { get; set; }

    public CheckIn? CheckIn { get; set; }
    public CheckOut? CheckOut { get; set; }
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    public Invoice? Invoice { get; set; }
}
