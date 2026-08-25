using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApi.Models;

[Table("user_role")]
public class UserRole
{
    [Column("user_id")]
    public long UserId { get; set; }

    public AppUser User { get; set; } = null!;

    [Column("role_id")]
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;
}
