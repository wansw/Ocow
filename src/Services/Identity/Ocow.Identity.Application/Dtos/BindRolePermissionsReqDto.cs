namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色绑定权限点请求 DTO。
/// </summary>
public class BindRolePermissionsReqDto
{
    public List<Guid> PermissionIds { get; init; } = new();
}
