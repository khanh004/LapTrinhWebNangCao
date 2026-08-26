namespace HotelManagement.Api.Entities;

public enum Gender
{
    MALE,
    FEMALE,
    OTHER
}

public class Customer
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IdentityNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
