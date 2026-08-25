using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolApi.Models;

[Table("role")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required, MaxLength(40)]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    public List<UserRole> UserRoles { get; set; } = [];
}
