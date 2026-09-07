namespace HotelManagement.Api.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceCode { get; set; } = null!;
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string RoomNumber { get; set; } = null!;
    public decimal RoomAmount { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal Surcharge { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Description { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
}