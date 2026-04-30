namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色绑定权限点请求 DTO。
/// </summary>
public class BindRolePermissionsReqDto
{
    /// <summary>
    /// 需要绑定到角色的权限点编号列表。
    /// </summary>
    public List<Guid> PermissionIds { get; init; } = new();
}
