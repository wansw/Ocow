namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 角色权限关联实体，用于绑定角色和权限点。
/// </summary>
public class RolePermissionModel
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }
}
