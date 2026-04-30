namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 管理员角色关联实体，用于绑定管理员和角色。
/// </summary>
public class AdminUserRoleModel
{
    public Guid AdminUserId { get; set; }

    public Guid RoleId { get; set; }
}
