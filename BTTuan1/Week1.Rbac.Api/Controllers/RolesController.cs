using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Week1.Rbac.Api.Contracts;
using Week1.Rbac.Api.Data;
using Week1.Rbac.Api.Models;

namespace Week1.Rbac.Api.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(
        CancellationToken ct)
    {
        var roles = await db.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .OrderBy(x => x.Name)
            .Select(x => new RoleResponse(
                x.Id,
                x.Name,
                x.Description,
                x.RolePermissions
                    .Select(rp => rp.Permission.Code)
                    .OrderBy(code => code)
                    .ToList()))
            .ToListAsync(ct);

        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var role = await db.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .Where(x => x.Id == id)
            .Select(x => new RoleResponse(
                x.Id,
                x.Name,
                x.Description,
                x.RolePermissions
                    .Select(rp => rp.Permission.Code)
                    .OrderBy(code => code)
                    .ToList()))
            .SingleOrDefaultAsync(ct);

        return role is null
            ? NotFound()
            : Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(
        CreateRoleRequest request,
        CancellationToken ct)
    {
        var name = request.Name.Trim().ToLowerInvariant();

        if (await db.Roles.AnyAsync(x => x.Name == name, ct))
            return Conflict("Role already exists.");

        var role = new Role
        {
            Name = name,
            Description = request.Description.Trim()
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);

        var response = new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            []);

        return CreatedAtAction(
            nameof(GetById),
            new { id = role.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> Update(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken ct)
    {
        var role = await db.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (role is null)
            return NotFound();

        var name = request.Name.Trim().ToLowerInvariant();

        if (await db.Roles.AnyAsync(
                x => x.Id != id && x.Name == name, ct))
        {
            return Conflict("Role already exists.");
        }

        role.Name = name;
        role.Description = request.Description.Trim();

        await db.SaveChangesAsync(ct);

        var response = new RoleResponse(
            role.Id,
            role.Name,
            role.Description,
            role.RolePermissions
                .Select(x => x.Permission.Code)
                .OrderBy(x => x)
                .ToList());

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var role = await db.Roles.FindAsync([id], ct);

        if (role is null)
            return NotFound();

        db.Roles.Remove(role);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> AssignPermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken ct)
    {
        var roleExists = await db.Roles
            .AnyAsync(x => x.Id == roleId, ct);

        if (!roleExists)
            return NotFound("Role not found.");

        var permissionExists = await db.Permissions
            .AnyAsync(x => x.Id == permissionId, ct);

        if (!permissionExists)
            return NotFound("Permission not found.");

        var exists = await db.RolePermissions.AnyAsync(
            x => x.RoleId == roleId &&
                 x.PermissionId == permissionId, ct);

        if (exists)
            return Conflict("Permission already assigned to role.");

        db.RolePermissions.Add(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemovePermission(
        Guid roleId,
        Guid permissionId,
        CancellationToken ct)
    {
        var relation = await db.RolePermissions
            .SingleOrDefaultAsync(
                x => x.RoleId == roleId &&
                     x.PermissionId == permissionId,
                ct);

        if (relation is null)
            return NotFound();

        db.RolePermissions.Remove(relation);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
}