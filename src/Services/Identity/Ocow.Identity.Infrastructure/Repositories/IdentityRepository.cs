using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Repositories;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Infrastructure.Repositories;

/// <summary>
/// 身份认证仓储 EF Core 实现，用于持久化管理员、角色、权限、菜单和 Token。
/// </summary>
public class IdentityRepository : EfRepositoryBase<IdentityDbContext, AdminUser, Guid>, IIdentityRepository
{
    public IdentityRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// 根据用户名查询管理员账号。
    /// </summary>
    public async Task<AdminUser?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await DbContext.AdminUsers.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// 根据管理员编号查询角色直接绑定的权限点编码。
    /// </summary>
    public async Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var roleIds = DbContext.AdminUserRoles
            .Where(x => x.AdminUserId == adminUserId)
            .Select(x => x.RoleId);

        return await DbContext.RolePermissions
            .Where(x => roleIds.Contains(x.RoleId))
            .Join(DbContext.Permissions, rolePermission => rolePermission.PermissionId, permission => permission.Id, (_, permission) => permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询管理员账号。
    /// </summary>
    public async Task<PageResDto<AdminUser>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var pageIndex = reqDto.GetSafePageIndex();
        var pageSize = reqDto.GetSafePageSize();
        var query = DbContext.AdminUsers.OrderByDescending(x => x.CreatedAt);

        return new PageResDto<AdminUser>
        {
            Items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
            Total = await query.LongCountAsync(cancellationToken),
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 新增管理员账号。
    /// </summary>
    public async Task AddAdminUserAsync(AdminUser adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        await AddAsync(adminUser, cancellationToken);
        await DbContext.AdminUserRoles.AddRangeAsync(roleIds.Select(roleId => new AdminUserRole
        {
            AdminUserId = adminUser.Id,
            RoleId = roleId
        }), cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 禁用管理员账号。
    /// </summary>
    public async Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var adminUser = await GetByIdAsync(id, cancellationToken) ??
                        throw new InvalidOperationException("管理员不存在。");
        adminUser.Status = AdminUserStatusEnum.Disabled;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 查询角色列表。
    /// </summary>
    public async Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Roles.OrderBy(x => x.Code).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 保存角色。
    /// </summary>
    public async Task<Role> SaveRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        var existing = await DbContext.Roles.FirstOrDefaultAsync(x => x.Id == role.Id, cancellationToken);
        if (existing is null)
        {
            await DbContext.Roles.AddAsync(role, cancellationToken);
        }
        else
        {
            existing.Code = role.Code;
            existing.Name = role.Name;
            role = existing;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Permissions.OrderBy(x => x.Module).ThenBy(x => x.Code).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 查询菜单列表。
    /// </summary>
    public async Task<IReadOnlyList<Menu>> GetMenusAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Menus
            .Include(x => x.Permission)
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 查询指定管理员按权限点可见的菜单列表。
    /// </summary>
    public async Task<IReadOnlyList<Menu>> GetAdminMenusAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var roleIds = DbContext.AdminUserRoles
            .Where(x => x.AdminUserId == adminUserId)
            .Select(x => x.RoleId);
        var permissionIds = DbContext.RolePermissions
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.PermissionId);

        return await DbContext.Menus
            .Include(x => x.Permission)
            .Where(x => x.IsEnabled && x.IsVisible && (x.PermissionId == null || permissionIds.Contains(x.PermissionId.Value)))
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 保存菜单。
    /// </summary>
    public async Task<Menu> SaveMenuAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        var existing = await DbContext.Menus.FirstOrDefaultAsync(x => x.Id == menu.Id, cancellationToken);
        if (existing is null)
        {
            await DbContext.Menus.AddAsync(menu, cancellationToken);
        }
        else
        {
            existing.ParentId = menu.ParentId;
            existing.Code = menu.Code;
            existing.Name = menu.Name;
            existing.Type = menu.Type;
            existing.Path = menu.Path;
            existing.Component = menu.Component;
            existing.Icon = menu.Icon;
            existing.Sort = menu.Sort;
            existing.PermissionId = menu.PermissionId;
            existing.IsVisible = menu.IsVisible;
            existing.IsEnabled = menu.IsEnabled;
            menu = existing;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
        return menu;
    }

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    public async Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var oldItems = DbContext.RolePermissions.Where(x => x.RoleId == roleId);
        DbContext.RolePermissions.RemoveRange(oldItems);
        await DbContext.RolePermissions.AddRangeAsync(permissionIds.Distinct().Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        }), cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 根据 openid 查询会员身份。
    /// </summary>
    public async Task<MemberIdentity?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
    {
        return await DbContext.MemberIdentities.FirstOrDefaultAsync(x => x.OpenId == openId, cancellationToken);
    }

    /// <summary>
    /// 保存会员身份。
    /// </summary>
    public async Task SaveMemberIdentityAsync(MemberIdentity memberIdentity, CancellationToken cancellationToken = default)
    {
        await DbContext.MemberIdentities.AddAsync(memberIdentity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 保存刷新 Token。
    /// </summary>
    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await DbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 根据刷新 Token 查询有效登录凭证。
    /// </summary>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
    {
        return await DbContext.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == token &&
            x.Scope == scope &&
            x.RevokedAt == null &&
            x.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    /// <summary>
    /// 吊销刷新 Token。
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
    {
        var refreshToken = await DbContext.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == token &&
            x.Scope == scope &&
            x.RevokedAt == null, cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 写入登录日志。
    /// </summary>
    public async Task AddLoginLogAsync(LoginLog loginLog, CancellationToken cancellationToken = default)
    {
        await DbContext.LoginLogs.AddAsync(loginLog, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
