using HotelManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<CheckOut> CheckOuts => Set<CheckOut>();

    public DbSet<Service> Services => Set<Service>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<RoomWaitlist> RoomWaitlist => Set<RoomWaitlist>();
    public DbSet<BookingLog> BookingLogs => Set<BookingLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- RBAC ----------
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.ToTable("permissions");
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions");
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("employees");
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Employee).WithOne(emp => emp.User)
                .HasForeignKey<User>(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Phòng ----------
        modelBuilder.Entity<RoomType>(e =>
        {
            e.ToTable("room_types");
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.PricePerNight).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<Room>(e =>
{
    e.ToTable("rooms");
    e.HasIndex(x => x.RoomNumber).IsUnique();
    e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    e.HasOne(x => x.RoomType).WithMany(rt => rt.Rooms)
        .HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
    e.HasOne(x => x.CleaningClaimedByEmployee).WithMany()
        .HasForeignKey(x => x.CleaningClaimedBy).OnDelete(DeleteBehavior.SetNull);
});

        // ---------- Khách hàng ----------
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasIndex(x => x.IdentityNumber).IsUnique();
            e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
        });

        // ---------- Đặt phòng / nhận / trả ----------
        modelBuilder.Entity<Booking>(e =>
        {
            e.ToTable("bookings");
            e.HasIndex(x => x.BookingCode).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Customer).WithMany(c => c.Bookings)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Room).WithMany(r => r.Bookings)
                .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckIn>(e =>
        {
            e.ToTable("check_ins");
            e.HasOne(x => x.Booking).WithOne(b => b.CheckIn)
                .HasForeignKey<CheckIn>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckOut>(e =>
        {
            e.ToTable("check_outs");
            e.HasOne(x => x.Booking).WithOne(b => b.CheckOut)
                .HasForeignKey<CheckOut>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Dịch vụ ----------
        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.Property(x => x.Price).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<BookingService>(e =>
        {
            e.ToTable("booking_services");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
            e.HasOne(x => x.Booking).WithMany(b => b.BookingServices)
                .HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Service).WithMany(s => s.BookingServices)
                .HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Hóa đơn ----------
        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("invoices");
            e.HasIndex(x => x.InvoiceCode).IsUnique();
            e.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.RoomAmount).HasColumnType("decimal(14,2)");
            e.Property(x => x.ServiceAmount).HasColumnType("decimal(14,2)");
            e.Property(x => x.Surcharge).HasColumnType("decimal(14,2)");
            e.Property(x => x.TotalAmount).HasColumnType("decimal(14,2)");
            e.HasOne(x => x.Booking).WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        });
                 // ---------- Hàng chờ & Nhật ký ----------
        modelBuilder.Entity<RoomWaitlist>(e =>
        {
            e.ToTable("room_waitlist");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingLog>(e =>
        {
            e.ToTable("booking_logs");
            e.Property(x => x.Action).HasConversion<string>().HasMaxLength(30);
            e.HasOne(x => x.Booking).WithMany().HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.PerformedBy).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
