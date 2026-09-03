namespace HotelManagement.Api.Entities;

public enum PaymentStatus
{
    UNPAID,
    PAID
}

public class Invoice
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public string InvoiceCode { get; set; } = null!;
    public decimal RoomAmount { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal Surcharge { get; set; }
    public decimal TotalAmount { get; set; }

    // Mô tả chi tiết từng dòng tiền: thuê gốc bao nhiêu đêm, gia hạn thêm bao nhiêu đêm/giờ, đơn giá
    public string? Description { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.UNPAID;
    public DateTime IssuedAt { get; set; }
}