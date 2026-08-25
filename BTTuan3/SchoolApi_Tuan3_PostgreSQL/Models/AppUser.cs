using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApi.Models;

[Table("app_user")]
public class AppUser
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required, MaxLength(160), EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("student_id")]
    public long? StudentId { get; set; }

    public Student? Student { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public List<UserRole> UserRoles { get; set; } = [];
}
