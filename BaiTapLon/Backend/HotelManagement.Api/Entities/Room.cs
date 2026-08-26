namespace HotelManagement.Api.Entities;

public enum RoomStatus
{
    AVAILABLE,
    RESERVED,
    OCCUPIED,
    MAINTENANCE,
    CLEANING
}

public class Room
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;

    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public RoomStatus Status { get; set; } = RoomStatus.AVAILABLE;
    public int? Floor { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}