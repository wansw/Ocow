using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 角色实体，用于承载 RBAC 角色信息。
/// </summary>
[Table("roles")]
public class Role
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 管理员角色绑定集合，用于表达角色与管理员的多对多关系。
    /// </summary>
    [InverseProperty(nameof(AdminUserRole.Role))]
    public List<AdminUserRole> AdminUserRoles { get; set; } = new();

    /// <summary>
    /// 角色权限绑定集合，用于表达角色与权限点的多对多关系。
    /// </summary>
    [InverseProperty(nameof(RolePermission.Role))]
    public List<RolePermission> RolePermissions { get; set; } = new();
}
