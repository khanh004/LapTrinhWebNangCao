using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface IInvoiceService
{
    Task<List<InvoiceDto>> GetAllAsync(string? paymentStatus);
    Task<InvoiceDto?> GetByIdAsync(Guid id);
    Task<bool> MarkAsPaidAsync(Guid id);
}

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static InvoiceDto ToDto(Invoice i) => new()
    {
        Id = i.Id,
        InvoiceCode = i.InvoiceCode,
        BookingId = i.BookingId,
        BookingCode = i.Booking?.BookingCode ?? string.Empty,
        CustomerName = i.Booking?.Customer?.FullName ?? string.Empty,
        RoomNumber = i.Booking?.Room?.RoomNumber ?? string.Empty,
        RoomAmount = i.RoomAmount,
        ServiceAmount = i.ServiceAmount,
        Surcharge = i.Surcharge,
        TotalAmount = i.TotalAmount,
        Description = i.Description,
        PaymentStatus = i.PaymentStatus.ToString(),
        IssuedAt = i.IssuedAt
    };

    public async Task<List<InvoiceDto>> GetAllAsync(string? paymentStatus)
    {
        var query = _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Customer)
            .Include(i => i.Booking).ThenInclude(b => b.Room)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(paymentStatus) &&
            Enum.TryParse<PaymentStatus>(paymentStatus, true, out var status))
        {
            query = query.Where(i => i.PaymentStatus == status);
        }

        return await query
            .OrderByDescending(i => i.IssuedAt)
            .Select(i => ToDto(i))
            .ToListAsync();
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Customer)
            .Include(i => i.Booking).ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(i => i.Id == id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<bool> MarkAsPaidAsync(Guid id)
    {
        var entity = await _context.Invoices.FindAsync(id);
        if (entity is null) return false;

        entity.PaymentStatus = PaymentStatus.PAID;
        await _context.SaveChangesAsync();
        return true;
    }
}