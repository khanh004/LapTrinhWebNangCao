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

    // Cơ chế "nhận dọn" - tránh 2 lao công cùng dọn 1 phòng
    public Guid? CleaningClaimedBy { get; set; }
    public Employee? CleaningClaimedByEmployee { get; set; }
    public DateTime? CleaningClaimedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}