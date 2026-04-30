using Microsoft.EntityFrameworkCore;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Domain.Enums;
using Ocow.Identity.Domain.Models;
using Ocow.Identity.Infrastructure.Data;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Infrastructure.Repositories;

/// <summary>
/// 身份认证仓储 EF Core 实现，用于持久化管理员、角色、权限和 Token。
/// </summary>
public class IdentityRepository : IIdentityRepository
{
    private readonly IdentityDbContext _dbContext;

    public IdentityRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 根据用户名查询管理员账号。
    /// </summary>
    public async Task<AdminUserModel?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AdminUsers.FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    /// <summary>
    /// 根据管理员编号查询权限点编码。
    /// </summary>
    public async Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var roleIds = _dbContext.AdminUserRoles
            .Where(x => x.AdminUserId == adminUserId)
            .Select(x => x.RoleId);

        return await _dbContext.RolePermissions
            .Where(x => roleIds.Contains(x.RoleId))
            .Join(_dbContext.Permissions, rp => rp.PermissionId, p => p.Id, (_, permission) => permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 分页查询管理员账号。
    /// </summary>
    public async Task<PageResDto<AdminUserModel>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var pageIndex = reqDto.GetSafePageIndex();
        var pageSize = reqDto.GetSafePageSize();
        var query = _dbContext.AdminUsers.OrderByDescending(x => x.CreatedAt);

        return new PageResDto<AdminUserModel>
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
    public async Task AddAdminUserAsync(AdminUserModel adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        await _dbContext.AdminUsers.AddAsync(adminUser, cancellationToken);
        await _dbContext.AdminUserRoles.AddRangeAsync(roleIds.Select(roleId => new AdminUserRoleModel
        {
            AdminUserId = adminUser.Id,
            RoleId = roleId
        }), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 禁用管理员账号。
    /// </summary>
    public async Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var adminUser = await _dbContext.AdminUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
                        throw new InvalidOperationException("管理员不存在。");
        adminUser.Status = AdminUserStatusEnum.Disabled;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 查询角色列表。
    /// </summary>
    public async Task<IReadOnlyList<RoleModel>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles.OrderBy(x => x.Code).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 保存角色。
    /// </summary>
    public async Task<RoleModel> SaveRoleAsync(RoleModel role, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == role.Id, cancellationToken);
        if (existing is null)
        {
            await _dbContext.Roles.AddAsync(role, cancellationToken);
        }
        else
        {
            existing.Code = role.Code;
            existing.Name = role.Name;
            role = existing;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    public async Task<IReadOnlyList<PermissionModel>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions.OrderBy(x => x.Module).ThenBy(x => x.Code).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    public async Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var oldItems = _dbContext.RolePermissions.Where(x => x.RoleId == roleId);
        _dbContext.RolePermissions.RemoveRange(oldItems);
        await _dbContext.RolePermissions.AddRangeAsync(permissionIds.Distinct().Select(permissionId => new RolePermissionModel
        {
            RoleId = roleId,
            PermissionId = permissionId
        }), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 根据 openid 查询会员身份。
    /// </summary>
    public async Task<MemberIdentityModel?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MemberIdentities.FirstOrDefaultAsync(x => x.OpenId == openId, cancellationToken);
    }

    /// <summary>
    /// 保存会员身份。
    /// </summary>
    public async Task SaveMemberIdentityAsync(MemberIdentityModel memberIdentity, CancellationToken cancellationToken = default)
    {
        await _dbContext.MemberIdentities.AddAsync(memberIdentity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 保存刷新 Token。
    /// </summary>
    public async Task SaveRefreshTokenAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 根据刷新 Token 查询有效登录凭证。
    /// </summary>
    public async Task<RefreshTokenModel?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x =>
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
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x =>
            x.Token == token &&
            x.Scope == scope &&
            x.RevokedAt == null, cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 写入登录日志。
    /// </summary>
    public async Task AddLoginLogAsync(LoginLogModel loginLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.LoginLogs.AddAsync(loginLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
