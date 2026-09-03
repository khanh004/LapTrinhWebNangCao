namespace HotelManagement.Api.Entities;

public enum BookingAction
{
    CREATED,
    CONFIRMED,
    CHECKED_IN,
    CHECKED_OUT,
    EARLY_CHECKOUT,
    EXTENDED,
    LATE_CHECKOUT,
    CANCELLED,
    AUTO_CANCELLED_NO_SHOW,
    LATE_ARRIVAL_NOTED
}

public class BookingLog
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public BookingAction Action { get; set; }

    public Guid PerformedBy { get; set; }
    public Employee Employee { get; set; } = null!;

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}