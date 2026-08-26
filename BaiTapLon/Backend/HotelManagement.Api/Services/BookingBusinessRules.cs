namespace HotelManagement.Api.Services;

public static class BookingBusinessRules
{
    public const int StandardCheckInHour = 14;
    public const int StandardCheckOutHour = 12;
    public const int LateCheckoutCutoffHour = 18;
    public const decimal LateCheckoutHourlyRate = 0.1m;

    public static int NightsBetween(DateOnly checkIn, DateOnly checkOut)
        => Math.Max(1, checkOut.DayNumber - checkIn.DayNumber);
}