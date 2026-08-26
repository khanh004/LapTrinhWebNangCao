using HotelManagement.Api.Data;
using HotelManagement.Api.DTOs;
using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(string? search);
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static CustomerDto ToDto(Customer c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        DateOfBirth = c.DateOfBirth,
        Gender = c.Gender?.ToString(),
        Phone = c.Phone,
        Email = c.Email,
        IdentityNumber = c.IdentityNumber,
        Address = c.Address
    };

    public async Task<List<CustomerDto>> GetAllAsync(string? search)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.FullName.Contains(search) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.IdentityNumber != null && c.IdentityNumber.Contains(search)));
        }

        return await query
            .OrderBy(c => c.FullName)
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Customers.FindAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        Enum.TryParse<Gender>(dto.Gender, true, out var parsedGender);

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            DateOfBirth = dto.DateOfBirth,
            Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : parsedGender,
            Phone = dto.Phone,
            Email = dto.Email,
            IdentityNumber = dto.IdentityNumber,
            Address = dto.Address,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(entity);
        await _context.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var entity = await _context.Customers.FindAsync(id);
        if (entity is null) return false;

        Enum.TryParse<Gender>(dto.Gender, true, out var parsedGender);

        entity.FullName = dto.FullName;
        entity.DateOfBirth = dto.DateOfBirth;
        entity.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : parsedGender;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.IdentityNumber = dto.IdentityNumber;
        entity.Address = dto.Address;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Customers.FindAsync(id);
        if (entity is null) return false;

        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}