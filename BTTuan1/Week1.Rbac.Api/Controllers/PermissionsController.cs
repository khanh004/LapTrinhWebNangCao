using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Week1.Rbac.Api.Contracts;
using Week1.Rbac.Api.Data;
using Week1.Rbac.Api.Models;

namespace Week1.Rbac.Api.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetAll(
        CancellationToken ct)
    {
        var permissions = await db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionResponse(
                x.Id,
                x.Code,
                x.Description))
            .ToListAsync(ct);

        return Ok(permissions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermissionResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var permission = await db.Permissions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PermissionResponse(
                x.Id,
                x.Code,
                x.Description))
            .SingleOrDefaultAsync(ct);

        return permission is null
            ? NotFound()
            : Ok(permission);
    }

    [HttpPost]
    public async Task<ActionResult<PermissionResponse>> Create(
        CreatePermissionRequest request,
        CancellationToken ct)
    {
        var code = request.Code.Trim().ToLowerInvariant();

        if (await db.Permissions.AnyAsync(
                x => x.Code == code, ct))
        {
            return Conflict("Permission code already exists.");
        }

        var permission = new Permission
        {
            Code = code,
            Description = request.Description.Trim()
        };

        db.Permissions.Add(permission);
        await db.SaveChangesAsync(ct);

        var response = new PermissionResponse(
            permission.Id,
            permission.Code,
            permission.Description);

        return CreatedAtAction(
            nameof(GetById),
            new { id = permission.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PermissionResponse>> Update(
        Guid id,
        UpdatePermissionRequest request,
        CancellationToken ct)
    {
        var permission = await db.Permissions.FindAsync([id], ct);

        if (permission is null)
            return NotFound();

        var code = request.Code.Trim().ToLowerInvariant();

        if (await db.Permissions.AnyAsync(
                x => x.Id != id && x.Code == code, ct))
        {
            return Conflict("Permission code already exists.");
        }

        permission.Code = code;
        permission.Description = request.Description.Trim();

        await db.SaveChangesAsync(ct);

        return Ok(new PermissionResponse(
            permission.Id,
            permission.Code,
            permission.Description));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var permission = await db.Permissions.FindAsync([id], ct);

        if (permission is null)
            return NotFound();

        db.Permissions.Remove(permission);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
}