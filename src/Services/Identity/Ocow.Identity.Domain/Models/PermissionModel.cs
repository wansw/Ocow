namespace Ocow.Identity.Domain.Models;

/// <summary>
/// 权限点实体，用于表达后台可授权的操作点。
/// </summary>
public class PermissionModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;
}
