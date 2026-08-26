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

        return await query
            .OrderBy(r => r.RoomNumber)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<RoomDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Rooms.Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
        return entity is null ? null : ToDto(entity);
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
}