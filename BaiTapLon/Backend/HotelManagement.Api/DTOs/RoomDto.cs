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

public class RoomAvailabilityDto
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string CurrentStatus { get; set; } = null!;
    public bool CanBookImmediately { get; set; }

    // TH1: đang có khách thuê / đã có người đặt trước -> ngày phòng sẽ trống
    public DateOnly? ConflictUntil { get; set; }

    // TH2: đang dọn dẹp -> thời điểm dự kiến dọn xong (giờ trả phòng + 30 phút)
    public DateTime? EstimatedCleaningReadyAt { get; set; }

    // TH3: gợi ý phòng khác cùng loại còn trống (khi bảo trì hoặc trùng lịch)
    public List<RoomDto> SuggestedAlternativeRooms { get; set; } = new();
}