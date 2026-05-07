using Ocow.Identity.Domain.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 身份认证仓储接口，用于隔离应用层和身份数据库实现
/// </summary>
public interface IIdentityRepository
{
    /// <summary>
    /// 根据用户名查询管理员账号。    /// </summary>
    Task<AdminUser?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据管理员编号查询权限点编码。    /// </summary>
    Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询管理员账号。    /// </summary>
    Task<PageResDto<AdminUser>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增管理员账号。    /// </summary>
    Task AddAdminUserAsync(AdminUser adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 禁用管理员账号。    /// </summary>
    Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询角色列表。    /// </summary>
    Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存角色。    /// </summary>
    Task<Role> SaveRoleAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询权限点列表。    /// </summary>
    Task<IReadOnlyList<Permission>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 绑定角色权限点。    /// </summary>
    Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 openid 查询会员身份。    /// </summary>
    Task<MemberIdentity?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存会员身份。    /// </summary>
    Task SaveMemberIdentityAsync(MemberIdentity memberIdentity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存刷新 Token。    /// </summary>
    Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据刷新 Token 查询有效登录凭证。    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// 吊销刷新 Token。    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入登录日志。    /// </summary>
    Task AddLoginLogAsync(LoginLog loginLog, CancellationToken cancellationToken = default);
}
