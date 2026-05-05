using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 角色权限关联实体，用于绑定角色和权限点。/// </summary>
[Table("role_permissions")]
public class RolePermission
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    /// <summary>
    /// 关联的角色，用于表达角色权限绑定的角色外键。    /// </summary>
    [ForeignKey(nameof(RoleId))]
    [InverseProperty(nameof(Role.RolePermissions))]
    public Role? Role { get; set; }

    /// <summary>
    /// 关联的权限点，用于表达角色权限绑定的权限外键。    /// </summary>
    [ForeignKey(nameof(PermissionId))]
    [InverseProperty(nameof(Permission.RolePermissions))]
    public Permission? Permission { get; set; }
}
