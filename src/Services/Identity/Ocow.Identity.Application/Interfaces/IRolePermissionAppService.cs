using Ocow.Identity.Application.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 角色权限应用服务接口，用于管理 RBAC 角色、权限点和后台菜单。
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
    /// 查询后台菜单树。
    /// </summary>
    Task<IReadOnlyList<MenuResDto>> GetMenusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询指定管理员按权限点可见的后台菜单树。
    /// </summary>
    Task<IReadOnlyList<MenuResDto>> GetAdminMenusAsync(Guid adminUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存后台菜单。
    /// </summary>
    Task<MenuResDto> SaveMenuAsync(Guid? id, MenuReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    Task BindRolePermissionsAsync(Guid roleId, BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken = default);
}
