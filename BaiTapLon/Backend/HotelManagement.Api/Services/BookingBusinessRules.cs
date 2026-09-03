namespace HotelManagement.Api.Services;

public static class BookingBusinessRules
{
    public const int StandardCheckInHour = 14;
    public const int StandardCheckOutHour = 12;
    public const int LateCheckoutCutoffHour = 18;
    public const decimal LateCheckoutHourlyRate = 0.1m;

    // Buffer thời gian tối thiểu (phút) để lao công dọn phòng trước khi phục vụ khách tiếp theo
    public const int HousekeepingBufferMinutes = 30;

    // Gia hạn thêm giờ tối đa được duyệt - tránh biến gia hạn giờ thành "đêm miễn phí" trá hình
    public const int MaxApprovedExtraHours = 6;

    // Số đêm charge thực tế - luôn tối thiểu 1 đêm (dùng khi tính hóa đơn)
    public static int NightsBetween(DateOnly checkIn, DateOnly checkOut)
        => Math.Max(1, checkOut.DayNumber - checkIn.DayNumber);

    // Số đêm thô giữa 2 mốc - cho phép bằng 0, dùng để tách "gốc" vs "gia hạn" khi tính hóa đơn
    public static int RawNightsBetween(DateOnly a, DateOnly b)
        => Math.Max(0, b.DayNumber - a.DayNumber);
}