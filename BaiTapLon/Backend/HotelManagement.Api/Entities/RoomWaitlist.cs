namespace HotelManagement.Api.Entities;

public enum WaitlistStatus
{
    WAITING,
    NOTIFIED,
    CONVERTED,
    CANCELLED
}

public class RoomWaitlist
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateOnly DesiredCheckIn { get; set; }
    public DateOnly DesiredCheckOut { get; set; }
    public WaitlistStatus Status { get; set; } = WaitlistStatus.WAITING;
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
}