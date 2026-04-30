namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 角色实体，用于承载 RBAC 角色信息。
/// </summary>
public class RoleModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
