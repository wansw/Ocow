using Ocow.Identity.Application.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 角色权限应用服务接口，用于管理 RBAC 角色和权限点。
/// </summary>
public interface IRolePermissionAppService
{
    /// <summary>
    /// 查询角色列表。
    /// </summary>
    Task<IReadOnlyList<RoleResDto>> GetRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存角色。
    /// </summary>
    Task<RoleResDto> SaveRoleAsync(Guid? id, RoleReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    Task<IReadOnlyList<PermissionResDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    Task BindRolePermissionsAsync(Guid roleId, BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken = default);
}
