using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface IRoomWaitlistService
{
    Task<List<WaitlistDto>> GetByRoomAsync(Guid roomId);
    Task<(WaitlistDto? Entry, string? Error)> JoinAsync(JoinWaitlistDto dto);
    Task<(BookingDto? Booking, string? Error)> ConvertToBookingAsync(Guid waitlistId, Guid performedBy);
    Task<bool> CancelAsync(Guid waitlistId);
    Task NotifyNextInQueueAsync(Guid roomId);
}

public class RoomWaitlistService : IRoomWaitlistService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingService _bookingService;

    public RoomWaitlistService(ApplicationDbContext context, IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    private static WaitlistDto ToDto(RoomWaitlist w) => new()
    {
        Id = w.Id,
        CustomerName = w.Customer?.FullName ?? string.Empty,
        RoomNumber = w.Room?.RoomNumber ?? string.Empty,
        DesiredCheckIn = w.DesiredCheckIn,
        DesiredCheckOut = w.DesiredCheckOut,
        Status = w.Status.ToString(),
        QueuePosition = w.QueuePosition,
        CreatedAt = w.CreatedAt
    };

    public async Task<List<WaitlistDto>> GetByRoomAsync(Guid roomId)
    {
        return await _context.RoomWaitlist
            .Include(w => w.Customer).Include(w => w.Room)
            .Where(w => w.RoomId == roomId && w.Status != WaitlistStatus.CANCELLED && w.Status != WaitlistStatus.CONVERTED)
            .OrderBy(w => w.QueuePosition)
            .Select(w => ToDto(w))
            .ToListAsync();
    }

    public async Task<(WaitlistDto? Entry, string? Error)> JoinAsync(JoinWaitlistDto dto)
    {
        var room = await _context.Rooms.FindAsync(dto.RoomId);
        if (room is null) return (null, "Phòng không tồn tại.");

        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
            return (null, "Khách hàng không tồn tại.");

        var currentCount = await _context.RoomWaitlist.CountAsync(w =>
            w.RoomId == dto.RoomId && w.Status == WaitlistStatus.WAITING);

        var entity = new RoomWaitlist
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            RoomId = dto.RoomId,
            DesiredCheckIn = dto.DesiredCheckIn,
            DesiredCheckOut = dto.DesiredCheckOut,
            Status = WaitlistStatus.WAITING,
            QueuePosition = currentCount + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomWaitlist.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(w => w.Customer).LoadAsync();
        await _context.Entry(entity).Reference(w => w.Room).LoadAsync();

        return (ToDto(entity), null);
    }

    public async Task NotifyNextInQueueAsync(Guid roomId)
    {
        var next = await _context.RoomWaitlist
            .Where(w => w.RoomId == roomId && w.Status == WaitlistStatus.WAITING)
            .OrderBy(w => w.QueuePosition)
            .FirstOrDefaultAsync();

        if (next is null) return;

        next.Status = WaitlistStatus.NOTIFIED;
        next.NotifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<(BookingDto? Booking, string? Error)> ConvertToBookingAsync(Guid waitlistId, Guid performedBy)
    {
        var entry = await _context.RoomWaitlist.FindAsync(waitlistId);
        if (entry is null) return (null, "Không tìm thấy yêu cầu chờ.");
        if (entry.Status is WaitlistStatus.CONVERTED or WaitlistStatus.CANCELLED)
            return (null, "Yêu cầu chờ này đã xử lý xong.");

        var (booking, error) = await _bookingService.CreateAsync(new CreateBookingDto
        {
            CustomerId = entry.CustomerId,
            RoomId = entry.RoomId,
            CheckInDate = entry.DesiredCheckIn,
            CheckOutDate = entry.DesiredCheckOut
        }, performedBy);

        if (error is not null) return (null, error);

        entry.Status = WaitlistStatus.CONVERTED;
        await _context.SaveChangesAsync();

        return (booking, null);
    }

    public async Task<bool> CancelAsync(Guid waitlistId)
    {
        var entry = await _context.RoomWaitlist.FindAsync(waitlistId);
        if (entry is null) return false;

        entry.Status = WaitlistStatus.CANCELLED;
        await _context.SaveChangesAsync();
        return true;
    }
}