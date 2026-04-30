using Ocow.Identity.Domain.Models;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// 身份认证仓储接口，用于隔离应用层和身份数据库实现。
/// </summary>
public interface IIdentityRepository
{
    /// <summary>
    /// 根据用户名查询管理员账号。
    /// </summary>
    Task<AdminUserModel?> GetAdminUserByNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据管理员编号查询权限点编码。
    /// </summary>
    Task<IReadOnlyList<string>> GetAdminPermissionCodesAsync(Guid adminUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询管理员账号。
    /// </summary>
    Task<PageResDto<AdminUserModel>> GetAdminUsersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增管理员账号。
    /// </summary>
    Task AddAdminUserAsync(AdminUserModel adminUser, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 禁用管理员账号。
    /// </summary>
    Task DisableAdminUserAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询角色列表。
    /// </summary>
    Task<IReadOnlyList<RoleModel>> GetRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存角色。
    /// </summary>
    Task<RoleModel> SaveRoleAsync(RoleModel role, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询权限点列表。
    /// </summary>
    Task<IReadOnlyList<PermissionModel>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 绑定角色权限点。
    /// </summary>
    Task BindRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 openid 查询会员身份。
    /// </summary>
    Task<MemberIdentityModel?> GetMemberIdentityByOpenIdAsync(string openId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存会员身份。
    /// </summary>
    Task SaveMemberIdentityAsync(MemberIdentityModel memberIdentity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 保存刷新 Token。
    /// </summary>
    Task SaveRefreshTokenAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据刷新 Token 查询有效登录凭证。
    /// </summary>
    Task<RefreshTokenModel?> GetRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// 吊销刷新 Token。
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入登录日志。
    /// </summary>
    Task AddLoginLogAsync(LoginLogModel loginLog, CancellationToken cancellationToken = default);
}
