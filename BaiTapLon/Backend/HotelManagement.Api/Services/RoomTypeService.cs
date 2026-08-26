using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface IRoomTypeService
{
    Task<List<RoomTypeDto>> GetAllAsync();
    Task<RoomTypeDto?> GetByIdAsync(Guid id);
    Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateRoomTypeDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class RoomTypeService : IRoomTypeService
{
    private readonly ApplicationDbContext _context;

    public RoomTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static RoomTypeDto ToDto(RoomType r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        PricePerNight = r.PricePerNight,
        MaxGuests = r.MaxGuests
    };

    public async Task<List<RoomTypeDto>> GetAllAsync()
    {
        return await _context.RoomTypes
            .OrderBy(r => r.Name)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<RoomTypeDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.RoomTypes.FindAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto)
    {
        var entity = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            PricePerNight = dto.PricePerNight,
            MaxGuests = dto.MaxGuests,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomTypes.Add(entity);
        await _context.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateRoomTypeDto dto)
    {
        var entity = await _context.RoomTypes.FindAsync(id);
        if (entity is null) return false;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.PricePerNight = dto.PricePerNight;
        entity.MaxGuests = dto.MaxGuests;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.RoomTypes.FindAsync(id);
        if (entity is null) return false;

        _context.RoomTypes.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
