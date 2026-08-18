using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Week1.Rbac.Api.Contracts;
using Week1.Rbac.Api.Data;
using Week1.Rbac.Api.Models;

namespace Week1.Rbac.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        CancellationToken ct)
    {
        var users = await db.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .OrderBy(x => x.Email)
            .Select(x => new UserResponse(
                x.Id,
                x.Email,
                x.DisplayName,
                x.IsActive,
                x.CreatedAt,
                x.UserRoles
                    .Select(ur => ur.Role.Name)
                    .OrderBy(r => r)
                    .ToList()))
            .ToListAsync(ct);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.Id == id)
            .Select(x => new UserResponse(
                x.Id,
                x.Email,
                x.DisplayName,
                x.IsActive,
                x.CreatedAt,
                x.UserRoles
                    .Select(ur => ur.Role.Name)
                    .OrderBy(r => r)
                    .ToList()))
            .SingleOrDefaultAsync(ct);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(
        CreateUserRequest request,
        CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            return Conflict("Email already exists.");

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim()
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(
            user,
            request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.CreatedAt,
            []);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken ct)
    {
        var user = await db.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
            return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(
                x => x.Id != id && x.Email == email, ct))
            return Conflict("Email already exists.");

        user.Email = email;
        user.DisplayName = request.DisplayName.Trim();
        user.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);

        var response = new UserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.IsActive,
            user.CreatedAt,
            user.UserRoles
                .Select(x => x.Role.Name)
                .OrderBy(x => x)
                .ToList());

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var user = await db.Users.FindAsync([id], ct);

        if (user is null)
            return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPut("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken ct)
    {
        var userExists = await db.Users
            .AnyAsync(x => x.Id == userId, ct);

        if (!userExists)
            return NotFound("User not found.");

        var roleExists = await db.Roles
            .AnyAsync(x => x.Id == roleId, ct);

        if (!roleExists)
            return NotFound("Role not found.");

        var exists = await db.UserRoles.AnyAsync(
            x => x.UserId == userId && x.RoleId == roleId, ct);

        if (exists)
            return Conflict("Role already assigned to user.");

        db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{userId:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken ct)
    {
        var relation = await db.UserRoles.SingleOrDefaultAsync(
            x => x.UserId == userId && x.RoleId == roleId, ct);

        if (relation is null)
            return NotFound();

        db.UserRoles.Remove(relation);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
}