using Microsoft.EntityFrameworkCore;
using SchoolApi.Data;
using SchoolApi.Dtos;
using SchoolApi.Models;

namespace SchoolApi.Services;

public sealed class ProgrammeService(AppDbContext db) : IProgrammeService
{
    public async Task<IReadOnlyList<ProgrammeResponseDto>> GetAllAsync(
        CancellationToken ct) =>
        await db.Programmes
            .AsNoTracking()
            .OrderBy(p => p.ProgrammeCode)
            .Select(p => new ProgrammeResponseDto(
                p.ProgrammeId,
                p.ProgrammeCode,
                p.ProgrammeName,
                p.DegreeLevel,
                p.DurationYears,
                p.Students.Count))
            .ToListAsync(ct);

    public async Task<ProgrammeResponseDto?> GetByIdAsync(
        long id, CancellationToken ct) =>
        await db.Programmes
            .AsNoTracking()
            .Where(p => p.ProgrammeId == id)
            .Select(p => new ProgrammeResponseDto(
                p.ProgrammeId,
                p.ProgrammeCode,
                p.ProgrammeName,
                p.DegreeLevel,
                p.DurationYears,
                p.Students.Count))
            .SingleOrDefaultAsync(ct);

    public async Task<ProgrammeResponseDto> CreateAsync(
        ProgrammeCreateDto dto, CancellationToken ct)
    {
        var code = dto.ProgrammeCode.Trim().ToUpperInvariant();

        if (await db.Programmes.AnyAsync(
                p => p.ProgrammeCode == code, ct))
        {
            throw new InvalidOperationException(
                "Programme code already exists.");
        }

        var entity = new Programme
        {
            ProgrammeCode = code,
            ProgrammeName = dto.ProgrammeName.Trim(),
            DegreeLevel = dto.DegreeLevel.Trim().ToUpperInvariant(),
            DurationYears = dto.DurationYears
        };

        db.Programmes.Add(entity);
        await db.SaveChangesAsync(ct);

        return new ProgrammeResponseDto(
            entity.ProgrammeId,
            entity.ProgrammeCode,
            entity.ProgrammeName,
            entity.DegreeLevel,
            entity.DurationYears,
            0);
    }

    public async Task<ProgrammeResponseDto?> UpdateAsync(
        long id, ProgrammeUpdateDto dto, CancellationToken ct)
    {
        var entity = await db.Programmes.FindAsync([id], ct);

        if (entity is null)
            return null;

        var code = dto.ProgrammeCode.Trim().ToUpperInvariant();

        if (await db.Programmes.AnyAsync(
                p => p.ProgrammeId != id &&
                     p.ProgrammeCode == code, ct))
        {
            throw new InvalidOperationException(
                "Programme code already exists.");
        }

        entity.ProgrammeCode = code;
        entity.ProgrammeName = dto.ProgrammeName.Trim();
        entity.DegreeLevel = dto.DegreeLevel.Trim().ToUpperInvariant();
        entity.DurationYears = dto.DurationYears;

        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(
        long id, CancellationToken ct)
    {
        var entity = await db.Programmes.FindAsync([id], ct);

        if (entity is null)
            return false;

        if (await db.Students.AnyAsync(
                s => s.ProgrammeId == id, ct))
        {
            throw new InvalidOperationException(
                "Cannot delete a programme that still has students.");
        }

        db.Programmes.Remove(entity);
        await db.SaveChangesAsync(ct);

        return true;
    }
}