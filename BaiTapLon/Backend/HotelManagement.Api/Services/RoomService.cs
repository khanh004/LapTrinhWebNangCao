using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync(string? status, string? search);
    Task<RoomDto?> GetByIdAsync(Guid id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateRoomDto dto);
    Task<bool> UpdateStatusAsync(Guid id, UpdateRoomStatusDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<(bool Success, string? Error)> ConfirmCleaningAsync(Guid roomId, Guid performedBy);
    Task<RoomAvailabilityDto?> CheckAvailabilityAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut);
}

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;
    private readonly IRoomWaitlistService _waitlistService;

    public RoomService(ApplicationDbContext context, IRoomWaitlistService waitlistService)
    {
        _context = context;
        _waitlistService = waitlistService;
    }

    private static RoomDto ToDto(Room r) => new()
    {
        Id = r.Id,
        RoomNumber = r.RoomNumber,
        RoomTypeId = r.RoomTypeId,
        RoomTypeName = r.RoomType?.Name ?? string.Empty,
        Status = r.Status.ToString(),
        Floor = r.Floor
    };

    // Gắn thêm thông tin booking hiện tại + hàng chờ vào danh sách RoomDto đã có, tránh N+1 query
    private async Task<List<RoomDto>> AttachDetailsAsync(List<Room> rooms)
    {
        var roomIds = rooms.Select(r => r.Id).ToList();

        var activeBookings = await _context.Bookings
            .Where(b => roomIds.Contains(b.RoomId) &&
                b.Status != BookingStatus.CANCELLED && b.Status != BookingStatus.CHECKED_OUT)
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();

        var waitlistEntries = await _context.RoomWaitlist
            .Include(w => w.Customer)
            .Where(w => roomIds.Contains(w.RoomId) &&
                (w.Status == WaitlistStatus.WAITING || w.Status == WaitlistStatus.NOTIFIED))
            .OrderBy(w => w.QueuePosition)
            .ToListAsync();

        return rooms.Select(r =>
        {
            var dto = ToDto(r);

            var currentBooking = activeBookings.FirstOrDefault(b => b.RoomId == r.Id);
            if (currentBooking is not null)
            {
                dto.CurrentBookingCheckIn = currentBooking.CheckInDate;
                dto.CurrentBookingCheckOut = currentBooking.CheckOutDate;
            }

            dto.WaitingCustomers = waitlistEntries
                .Where(w => w.RoomId == r.Id)
                .Select(w => new WaitlistDto
                {
                    Id = w.Id,
                    CustomerName = w.Customer.FullName,
                    RoomNumber = r.RoomNumber,
                    DesiredCheckIn = w.DesiredCheckIn,
                    DesiredCheckOut = w.DesiredCheckOut,
                    Status = w.Status.ToString(),
                    QueuePosition = w.QueuePosition,
                    CreatedAt = w.CreatedAt
                }).ToList();

            return dto;
        }).ToList();
    }

    public async Task<List<RoomDto>> GetAllAsync(string? status, string? search)
    {
        var query = _context.Rooms.Include(r => r.RoomType).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<RoomStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.RoomNumber.Contains(search));
        }

        var rooms = await query.OrderBy(r => r.RoomNumber).ToListAsync();
        return await AttachDetailsAsync(rooms);
    }

    public async Task<RoomDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Rooms.Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (entity is null) return null;

        var list = await AttachDetailsAsync(new List<Room> { entity });
        return list.First();
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var entity = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = dto.RoomNumber,
            RoomTypeId = dto.RoomTypeId,
            Floor = dto.Floor,
            Status = RoomStatus.AVAILABLE,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(r => r.RoomType).LoadAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateRoomDto dto)
    {
        var entity = await _context.Rooms.FindAsync(id);
        if (entity is null) return false;

        entity.RoomNumber = dto.RoomNumber;
        entity.RoomTypeId = dto.RoomTypeId;
        entity.Floor = dto.Floor;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, UpdateRoomStatusDto dto)
    {
        var entity = await _context.Rooms.FindAsync(id);
        if (entity is null) return false;

        if (!Enum.TryParse<RoomStatus>(dto.Status, true, out var parsedStatus))
            return false;

        entity.Status = parsedStatus;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Rooms.FindAsync(id);
        if (entity is null) return false;

        _context.Rooms.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> ConfirmCleaningAsync(Guid roomId, Guid performedBy)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room is null) return (false, "Phòng không tồn tại.");
        if (room.Status != RoomStatus.CLEANING)
            return (false, "Phòng không ở trạng thái đang dọn dẹp.");

        room.Status = RoomStatus.AVAILABLE;
        await _context.SaveChangesAsync();

        await _waitlistService.NotifyNextInQueueAsync(roomId);

        return (true, null);
    }
    public async Task<RoomAvailabilityDto?> CheckAvailabilityAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut)
{
    var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.Id == roomId);
    if (room is null) return null;

    var result = new RoomAvailabilityDto
    {
        RoomId = room.Id,
        RoomNumber = room.RoomNumber,
        CurrentStatus = room.Status.ToString()
    };

    // TH3: Phòng đang bảo trì
    if (room.Status == RoomStatus.MAINTENANCE)
    {
        result.CanBookImmediately = false;
        result.SuggestedAlternativeRooms = await GetAlternativeRoomsAsync(room.RoomTypeId, checkIn, checkOut, roomId);
        return result;
    }

    // TH2: Phòng đang dọn dẹp -> tính giờ dự kiến xong (giờ trả phòng + 30 phút)
    if (room.Status == RoomStatus.CLEANING)
    {
        var lastCheckOutTime = await _context.CheckOuts
            .Where(c => c.Booking.RoomId == roomId)
            .OrderByDescending(c => c.ActualCheckOut)
            .Select(c => (DateTime?)c.ActualCheckOut)
            .FirstOrDefaultAsync();

        result.EstimatedCleaningReadyAt = lastCheckOutTime?.AddMinutes(30);
    }

    // TH1: Kiểm tra trùng lịch với booking khác
    var conflicting = await _context.Bookings
        .Where(b => b.RoomId == roomId &&
            b.Status != BookingStatus.CANCELLED && b.Status != BookingStatus.CHECKED_OUT &&
            b.CheckInDate < checkOut && b.CheckOutDate > checkIn)
        .OrderByDescending(b => b.CheckOutDate)
        .FirstOrDefaultAsync();

    if (conflicting is not null)
    {
        result.CanBookImmediately = false;
        result.ConflictUntil = conflicting.CheckOutDate;
        result.SuggestedAlternativeRooms = await GetAlternativeRoomsAsync(room.RoomTypeId, checkIn, checkOut, roomId);
    }
    else if (room.Status != RoomStatus.MAINTENANCE)
    {
        result.CanBookImmediately = true;
    }

    return result;
}

private async Task<List<RoomDto>> GetAlternativeRoomsAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, Guid excludeRoomId)
{
    var candidates = await _context.Rooms
        .Include(r => r.RoomType)
        .Where(r => r.RoomTypeId == roomTypeId && r.Id != excludeRoomId && r.Status != RoomStatus.MAINTENANCE)
        .ToListAsync();

    var available = new List<RoomDto>();
    foreach (var r in candidates)
    {
        var hasConflict = await _context.Bookings.AnyAsync(b =>
            b.RoomId == r.Id && b.Status != BookingStatus.CANCELLED && b.Status != BookingStatus.CHECKED_OUT &&
            b.CheckInDate < checkOut && b.CheckOutDate > checkIn);
        if (!hasConflict && r.Status != RoomStatus.CLEANING)
            available.Add(ToDto(r));
    }
    return available;
}
}