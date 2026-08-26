using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync()) return; // đã seed rồi thì bỏ qua toàn bộ

        // ---------- Roles ----------
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Quản trị viên - toàn quyền hệ thống", CreatedAt = DateTime.UtcNow };
        var receptionistRole = new Role { Id = Guid.NewGuid(), Name = "Receptionist", Description = "Nhân viên lễ tân", CreatedAt = DateTime.UtcNow };
        var housekeepingRole = new Role { Id = Guid.NewGuid(), Name = "Housekeeping", Description = "Nhân viên buồng phòng - xác nhận dọn phòng xong", CreatedAt = DateTime.UtcNow };
        context.Roles.AddRange(adminRole, receptionistRole, housekeepingRole);

        // ---------- Permissions ----------
        var permBooking = new Permission { Id = Guid.NewGuid(), Code = "BOOKING_MANAGE", Description = "Quản lý đặt phòng", CreatedAt = DateTime.UtcNow };
        var permCleaning = new Permission { Id = Guid.NewGuid(), Code = "ROOM_CLEANING_CONFIRM", Description = "Xác nhận dọn phòng xong", CreatedAt = DateTime.UtcNow };
        var permWaitlist = new Permission { Id = Guid.NewGuid(), Code = "WAITLIST_MANAGE", Description = "Quản lý hàng chờ đặt phòng", CreatedAt = DateTime.UtcNow };
        context.Permissions.AddRange(permBooking, permCleaning, permWaitlist);

        // ---------- Role - Permission ----------
        context.RolePermissions.AddRange(
            new RolePermission { RoleId = adminRole.Id, PermissionId = permBooking.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = permCleaning.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = permWaitlist.Id },
            new RolePermission { RoleId = receptionistRole.Id, PermissionId = permBooking.Id },
            new RolePermission { RoleId = receptionistRole.Id, PermissionId = permWaitlist.Id },
            new RolePermission { RoleId = housekeepingRole.Id, PermissionId = permCleaning.Id }
        );

        // ---------- Room Types (dữ liệu nền cố định, ngoài phạm vi phát triển) ----------
        var standard = new RoomType { Id = Guid.NewGuid(), Name = "Standard", Description = "Phòng tiêu chuẩn", PricePerNight = 500000, MaxGuests = 2, CreatedAt = DateTime.UtcNow };
        var deluxe = new RoomType { Id = Guid.NewGuid(), Name = "Deluxe", Description = "Phòng cao cấp", PricePerNight = 800000, MaxGuests = 2, CreatedAt = DateTime.UtcNow };
        var suite = new RoomType { Id = Guid.NewGuid(), Name = "Suite", Description = "Phòng suite", PricePerNight = 1500000, MaxGuests = 3, CreatedAt = DateTime.UtcNow };
        context.RoomTypes.AddRange(standard, deluxe, suite);

        // ---------- Employees & Users (tài khoản demo) ----------
        var admin = new Employee { Id = Guid.NewGuid(), FullName = "Quản trị viên", Position = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow };
        var receptionist = new Employee { Id = Guid.NewGuid(), FullName = "Nguyễn Văn Lễ Tân", Position = "Receptionist", IsActive = true, CreatedAt = DateTime.UtcNow };
        var housekeeper = new Employee { Id = Guid.NewGuid(), FullName = "Trần Thị Buồng Phòng", Position = "Housekeeping", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Employees.AddRange(admin, receptionist, housekeeper);

        var adminUser = new User { Id = Guid.NewGuid(), Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), EmployeeId = admin.Id, IsActive = true, CreatedAt = DateTime.UtcNow };
        var receptionistUser = new User { Id = Guid.NewGuid(), Username = "letan", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), EmployeeId = receptionist.Id, IsActive = true, CreatedAt = DateTime.UtcNow };
        var housekeeperUser = new User { Id = Guid.NewGuid(), Username = "buongphong", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), EmployeeId = housekeeper.Id, IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(adminUser, receptionistUser, housekeeperUser);

        context.UserRoles.AddRange(
            new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id },
            new UserRole { UserId = receptionistUser.Id, RoleId = receptionistRole.Id },
            new UserRole { UserId = housekeeperUser.Id, RoleId = housekeepingRole.Id }
        );

        await context.SaveChangesAsync();
    }
}