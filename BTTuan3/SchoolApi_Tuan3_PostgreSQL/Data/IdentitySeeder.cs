using Microsoft.EntityFrameworkCore;
using SchoolApi.Models;
using SchoolApi.Services;

namespace SchoolApi.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        IPasswordService passwords,
        IConfiguration config,
        CancellationToken ct = default)
    {
        foreach (var name in new[] { "Admin", "Staff", "Student" })
        {
            if (!await db.Roles.AnyAsync(r => r.RoleName == name, ct))
            {
                db.Roles.Add(new Role { RoleName = name });
            }
        }

        await db.SaveChangesAsync(ct);

        var seedPassword = config["Seed:Password"]
            ?? throw new InvalidOperationException(
                "Thieu Seed:Password trong User Secrets.");

        var accounts = new[]
        {
            ("admin@hnmu.edu.vn", "Admin"),
            ("staff@hnmu.edu.vn", "Staff"),
            ("sv001@hnmu.edu.vn", "Student")
        };

        foreach (var (email, roleName) in accounts)
        {
            var user = await db.AppUsers
                .Include(u => u.UserRoles)
                .SingleOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
            {
                user = new AppUser
                {
                    Email = email,
                    IsActive = true
                };

                user.PasswordHash = passwords.Hash(user, seedPassword);
                db.AppUsers.Add(user);
                await db.SaveChangesAsync(ct);
            }

            if (roleName == "Student" && user.StudentId is null)
            {
                user.StudentId = await db.Students
                    .OrderBy(s => s.StudentId)
                    .Select(s => (long?)s.StudentId)
                    .FirstOrDefaultAsync(ct);

                await db.SaveChangesAsync(ct);
            }

            var roleId = await db.Roles
                .Where(r => r.RoleName == roleName)
                .Select(r => r.RoleId)
                .SingleAsync(ct);

            var hasRole = await db.UserRoles.AnyAsync(
                ur => ur.UserId == user.UserId && ur.RoleId == roleId,
                ct);

            if (!hasRole)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = roleId
                });

                await db.SaveChangesAsync(ct);
            }
        }
    }
}
