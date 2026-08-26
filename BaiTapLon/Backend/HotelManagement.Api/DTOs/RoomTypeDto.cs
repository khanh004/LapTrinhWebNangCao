namespace HotelManagement.Api.DTOs;

public class RoomTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
}

public class CreateRoomTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
}

public class UpdateRoomTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
}
