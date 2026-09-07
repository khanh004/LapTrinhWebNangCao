namespace HotelManagement.Api.Services;

public static class BookingBusinessRules
{
    public const int StandardCheckInHour = 14;
    public const int StandardCheckOutHour = 12;
    public const int LateCheckoutCutoffHour = 18;
    public const decimal LateCheckoutHourlyRate = 0.1m;
    public const int HousekeepingBufferMinutes = 30;
    public const int MaxApprovedExtraHours = 6;

    // Quá giờ nhận phòng chuẩn bao nhiêu phút thì coi là no-show, tự động hủy
    public const int NoShowGraceMinutesAfterStandardCheckIn = 60;

    public static int NightsBetween(DateOnly checkIn, DateOnly checkOut)
        => Math.Max(1, checkOut.DayNumber - checkIn.DayNumber);

    public static int RawNightsBetween(DateOnly a, DateOnly b)
        => Math.Max(0, b.DayNumber - a.DayNumber);

    // Mốc deadline mặc định: ngày nhận phòng, lúc giờ chuẩn + số phút gia hạn no-show
    public static DateTime CalculateDefaultArrivalDeadline(DateOnly checkInDate)
{
    var localDateTime = checkInDate.ToDateTime(new TimeOnly(StandardCheckInHour, 0))
        .AddMinutes(NoShowGraceMinutesAfterStandardCheckIn);
    return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
}
}