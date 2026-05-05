using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 管理员角色关联实体，用于绑定管理员和角色。/// </summary>
[Table("admin_user_roles")]
public class AdminUserRole
{
    public Guid AdminUserId { get; set; }

    public Guid RoleId { get; set; }

    /// <summary>
    /// 关联的管理员，用于表达管理员角色绑定的管理员外键。    /// </summary>
    [ForeignKey(nameof(AdminUserId))]
    [InverseProperty(nameof(AdminUser.AdminUserRoles))]
    public AdminUser? AdminUser { get; set; }

    /// <summary>
    /// 关联的角色，用于表达管理员角色绑定的角色外键。    /// </summary>
    [ForeignKey(nameof(RoleId))]
    [InverseProperty(nameof(Role.AdminUserRoles))]
    public Role? Role { get; set; }
}
