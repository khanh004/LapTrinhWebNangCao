namespace HotelManagement.Api.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = null!;
    public string BookingCode { get; set; } = null!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public DateOnly OriginalCheckOutDate { get; set; }
    public int ApprovedExtraHours { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }
}

public class CreateBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
}

public class ExtendBookingDto
{
    // "DAYS" (thêm đêm) hoặc "HOURS" (thêm giờ trong ngày trả dự kiến)
    public string ExtensionType { get; set; } = "DAYS";

    // Bắt buộc nếu ExtensionType = "DAYS"
    public DateOnly? NewCheckOutDate { get; set; }

    // Bắt buộc nếu ExtensionType = "HOURS" (tối đa BookingBusinessRules.MaxApprovedExtraHours)
    public int? AdditionalHours { get; set; }
}

public class CheckOutRequestDto
{
    public DateTime? ActualCheckOutTime { get; set; }
}

public class BookingLogDto
{
    public string Action { get; set; } = null!;
    public string PerformedByName { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}