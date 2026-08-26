namespace HotelManagement.Api.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? Floor { get; set; }
}

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public int? Floor { get; set; }
}

public class UpdateRoomDto
{
    public string RoomNumber { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public int? Floor { get; set; }
}

public class UpdateRoomStatusDto
{
    public string Status { get; set; } = null!; // AVAILABLE, RESERVED, OCCUPIED, MAINTENANCE
}