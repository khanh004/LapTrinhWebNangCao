namespace HotelManagement.Api.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? Floor { get; set; }
    public decimal PricePerNight { get; set; }

    public DateOnly? CurrentBookingCheckIn { get; set; }
    public DateOnly? CurrentBookingCheckOut { get; set; }
    public List<WaitlistDto> WaitingCustomers { get; set; } = new();

    public Guid? CleaningClaimedByEmployeeId { get; set; }
    public string? CleaningClaimedByName { get; set; }
    public DateTime? CleaningClaimedAt { get; set; }
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
    public string Status { get; set; } = null!;
}

public class RoomAvailabilityDto
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string CurrentStatus { get; set; } = null!;
    public bool CanBookImmediately { get; set; }
    public DateOnly? ConflictUntil { get; set; }
    public DateTime? EstimatedCleaningReadyAt { get; set; }
    public List<RoomDto> SuggestedAlternativeRooms { get; set; } = new();
}