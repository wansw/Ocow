using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Models;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// 角色权限应用服务，用于管理 RBAC 角色、权限点和后台菜单。
/// </summary>
public class RolePermissionAppService : IRolePermissionAppService
{
    private readonly IIdentityRepository _repository;

    /// <summary>
    /// 创建角色权限应用服务。
    /// </summary>
    public RolePermissionAppService(IIdentityRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 查询角色列表。
    /// </summary>
    public async Task<IReadOnlyList<RoleResDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _repository.GetRolesAsync(cancellationToken);
        return roles.Select(MapRoleToResDto).ToList();
    }

    /// <summary>
    /// 保存角色。
    /// </summary>
    public async Task<RoleResDto> SaveRoleAsync(Guid? id, RoleReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var role = new Role
        {
            Id = id ?? Guid.NewGuid(),
            Code = reqDto.Code,
            Name = reqDto.Name
        };

        var result = await _repository.SaveRoleAsync(role, cancellationToken);
        return MapRoleToResDto(result);
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    public async Task<IReadOnlyList<PermissionResDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _repository.GetPermissionsAsync(cancellationToken);
        return permissions.Select(x => new PermissionResDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Module = x.Module
        }).ToList();
    }

    /// <summary>
    /// 查询后台菜单树。
    /// </summary>
    public async Task<IReadOnlyList<MenuResDto>> GetMenusAsync(CancellationToken cancellationToken = default)
    {
        var menus = await _repository.GetMenusAsync(cancellationToken);
        return BuildMenuTree(menus, pruneEmptyPermissionlessMenus: false);
    }

    /// <summary>
    /// 查询指定管理员按权限点可见的后台菜单树。
    /// </summary>
    public async Task<IReadOnlyList<MenuResDto>> GetAdminMenusAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var menus = await _repository.GetAdminMenusAsync(adminUserId, cancellationToken);
        return BuildMenuTree(menus, pruneEmptyPermissionlessMenus: true);
    }

    /// <summary>
    /// 保存后台菜单。
    /// </summary>
    public async Task<MenuResDto> SaveMenuAsync(Guid? id, MenuReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var menu = new Menu
        {
            Id = id ?? Guid.NewGuid(),
            ParentId = reqDto.ParentId,
            Code = reqDto.Code,
            Name = reqDto.Name,
            Type = reqDto.Type,
            Path = reqDto.Path,
            Component = reqDto.Component,
            Icon = reqDto.Icon,
            Sort = reqDto.Sort,
            PermissionId = reqDto.PermissionId,
            IsVisible = reqDto.IsVisible,
            IsEnabled = reqDto.IsEnabled
        };

        var result = await _repository.SaveMenuAsync(menu, cancellationToken);
        return MapMenuToResDto(result);
    }

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    public async Task BindRolePermissionsAsync(Guid roleId, BindRolePermissionsReqDto reqDto, CancellationToken cancellationToken = default)
    {
        await _repository.BindRolePermissionsAsync(roleId, reqDto.PermissionIds, cancellationToken);
    }

    /// <summary>
    /// 将角色实体转换为响应 DTO。
    /// </summary>
    private static RoleResDto MapRoleToResDto(Role role)
    {
        return new RoleResDto
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name
        };
    }

    /// <summary>
    /// 将菜单列表转换为菜单树。
    /// </summary>
    private static IReadOnlyList<MenuResDto> BuildMenuTree(IReadOnlyList<Menu> menus, bool pruneEmptyPermissionlessMenus)
    {
        var dtoMap = menus.ToDictionary(x => x.Id, MapMenuToResDto);
        var roots = new List<MenuResDto>();

        foreach (var menu in menus.OrderBy(x => x.Sort).ThenBy(x => x.Code))
        {
            var dto = dtoMap[menu.Id];
            if (menu.ParentId.HasValue && dtoMap.TryGetValue(menu.ParentId.Value, out var parent))
            {
                parent.Children.Add(dto);
                continue;
            }

            roots.Add(dto);
        }

        return pruneEmptyPermissionlessMenus ? PruneEmptyPermissionlessMenus(roots) : roots;
    }

    /// <summary>
    /// 裁剪没有权限点且没有可见子节点的菜单容器。
    /// </summary>
    private static IReadOnlyList<MenuResDto> PruneEmptyPermissionlessMenus(IReadOnlyList<MenuResDto> menus)
    {
        var result = new List<MenuResDto>();
        foreach (var menu in menus)
        {
            var children = PruneEmptyPermissionlessMenus(menu.Children);
            menu.Children.Clear();
            menu.Children.AddRange(children);

            if (menu.PermissionId.HasValue || menu.Children.Count > 0)
            {
                result.Add(menu);
            }
        }

        return result;
    }

    /// <summary>
    /// 将菜单实体转换为响应 DTO。
    /// </summary>
    private static MenuResDto MapMenuToResDto(Menu menu)
    {
        return new MenuResDto
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            Code = menu.Code,
            Name = menu.Name,
            Type = menu.Type,
            Path = menu.Path,
            Component = menu.Component,
            Icon = menu.Icon,
            Sort = menu.Sort,
            PermissionId = menu.PermissionId,
            PermissionCode = menu.Permission?.Code,
            IsVisible = menu.IsVisible,
            IsEnabled = menu.IsEnabled
        };
    }
}
