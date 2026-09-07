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
    Task<(bool Success, string? Error)> ConfirmCleaningAsync(Guid roomId, Guid performedBy, bool isAdmin);
    Task<(bool Success, string? Error)> ClaimCleaningAsync(Guid roomId, Guid employeeId);
    Task<(bool Success, string? Error)> ReleaseCleaningClaimAsync(Guid roomId, Guid employeeId, bool isAdmin);
    Task<RoomAvailabilityDto?> CheckAvailabilityAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut);
    Task<List<RoomDto>> SearchAvailableAsync(DateOnly checkIn, DateOnly checkOut);
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
        Floor = r.Floor,
        PricePerNight = r.RoomType?.PricePerNight ?? 0,
        CleaningClaimedByEmployeeId = r.CleaningClaimedBy,
        CleaningClaimedByName = r.CleaningClaimedByEmployee?.FullName,
        CleaningClaimedAt = r.CleaningClaimedAt
    };

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
        var query = _context.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.CleaningClaimedByEmployee)
            .AsQueryable();

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
        var entity = await _context.Rooms
            .Include(r => r.RoomType)
            .Include(r => r.CleaningClaimedByEmployee)
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

    public async Task<(bool Success, string? Error)> ConfirmCleaningAsync(Guid roomId, Guid performedBy, bool isAdmin)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room is null) return (false, "Phòng không tồn tại.");
        if (room.Status != RoomStatus.CLEANING)
            return (false, "Phòng không ở trạng thái đang dọn dẹp.");
        if (room.CleaningClaimedBy.HasValue && room.CleaningClaimedBy != performedBy && !isAdmin)
            return (false, "Phòng này đang được nhân viên khác nhận dọn, bạn không thể xác nhận thay.");

        room.Status = RoomStatus.AVAILABLE;
        room.CleaningClaimedBy = null;
        room.CleaningClaimedAt = null;
        await _context.SaveChangesAsync();

        await _waitlistService.NotifyNextInQueueAsync(roomId);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ClaimCleaningAsync(Guid roomId, Guid employeeId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room is null) return (false, "Phòng không tồn tại.");
        if (room.Status != RoomStatus.CLEANING)
            return (false, "Phòng không ở trạng thái đang dọn dẹp.");
        if (room.CleaningClaimedBy.HasValue)
            return (false, "Phòng này đã có nhân viên khác nhận dọn.");

        room.CleaningClaimedBy = employeeId;
        room.CleaningClaimedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ReleaseCleaningClaimAsync(Guid roomId, Guid employeeId, bool isAdmin)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room is null) return (false, "Phòng không tồn tại.");
        if (!room.CleaningClaimedBy.HasValue)
            return (false, "Phòng này chưa có ai nhận dọn.");
        if (room.CleaningClaimedBy != employeeId && !isAdmin)
            return (false, "Bạn không phải người đã nhận dọn phòng này.");

        room.CleaningClaimedBy = null;
        room.CleaningClaimedAt = null;
        await _context.SaveChangesAsync();
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

        if (room.Status == RoomStatus.MAINTENANCE)
        {
            result.CanBookImmediately = false;
            result.SuggestedAlternativeRooms = await GetAlternativeRoomsAsync(room.RoomTypeId, checkIn, checkOut, roomId);
            return result;
        }

        if (room.Status == RoomStatus.CLEANING)
        {
            var lastCheckOutTime = await _context.CheckOuts
                .Where(c => c.Booking.RoomId == roomId)
                .OrderByDescending(c => c.ActualCheckOut)
                .Select(c => (DateTime?)c.ActualCheckOut)
                .FirstOrDefaultAsync();

            result.EstimatedCleaningReadyAt = lastCheckOutTime?.AddMinutes(30);
        }

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

    // Tìm mọi phòng còn trống trong khoảng ngày yêu cầu - dùng cho tra cứu lịch tổng quát
    public async Task<List<RoomDto>> SearchAvailableAsync(DateOnly checkIn, DateOnly checkOut)
    {
        var rooms = await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.Status != RoomStatus.MAINTENANCE)
            .ToListAsync();
        var roomIds = rooms.Select(r => r.Id).ToList();

        var conflictingRoomIds = await _context.Bookings
            .Where(b => roomIds.Contains(b.RoomId) &&
                b.Status != BookingStatus.CANCELLED && b.Status != BookingStatus.CHECKED_OUT &&
                b.CheckInDate < checkOut && b.CheckOutDate > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var isImmediate = checkIn <= today;

        // Nếu khách muốn nhận phòng ngay hôm nay (hoặc quá khứ), phòng phải đang thực sự
        // AVAILABLE ngay lúc này. Nếu đặt cho tương lai thì trạng thái hiện tại không quan trọng,
        // chỉ cần không trùng lịch đặt nào khác trong khoảng ngày đó.
        var available = rooms.Where(r =>
            !conflictingRoomIds.Contains(r.Id) &&
            (!isImmediate || r.Status == RoomStatus.AVAILABLE));

        return available.OrderBy(r => r.RoomNumber).Select(r => ToDto(r)).ToList();
    }
}