namespace HotelManagement.Api.DTOs;

public class JoinWaitlistDto
{
    public Guid CustomerId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly DesiredCheckIn { get; set; }
    public DateOnly DesiredCheckOut { get; set; }
}

public class WaitlistDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public string RoomNumber { get; set; } = null!;
    public DateOnly DesiredCheckIn { get; set; }
    public DateOnly DesiredCheckOut { get; set; }
    public string Status { get; set; } = null!;
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
}