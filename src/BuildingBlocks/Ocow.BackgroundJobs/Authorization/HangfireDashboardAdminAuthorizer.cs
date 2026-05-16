using System.Security.Claims;

namespace Ocow.BackgroundJobs.Authorization;

/// <summary>
/// Hangfire Dashboard 后台管理员授权器，用于基于 Admin scope 和权限点声明判断访问资格。
/// </summary>
public class HangfireDashboardAdminAuthorizer
{
    /// <summary>
    /// 默认 Dashboard 访问权限点编码。
    /// </summary>
    public const string DefaultPermissionCode = "scheduler.trigger";

    /// <summary>
    /// 后台管理员 JWT scope 声明值。
    /// </summary>
    public const string AdminScope = "admin";

    private readonly string _permissionCode;

    /// <summary>
    /// 创建 Hangfire Dashboard 后台管理员授权器。
    /// </summary>
    public HangfireDashboardAdminAuthorizer(string permissionCode = DefaultPermissionCode)
    {
        _permissionCode = permissionCode;
    }

    /// <summary>
    /// 判断当前用户是否具备访问 Hangfire Dashboard 的后台权限。
    /// </summary>
    public bool IsAuthorized(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var hasAdminScope = user.Claims.Any(x =>
            string.Equals(x.Type, "scope", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Value, AdminScope, StringComparison.OrdinalIgnoreCase));
        var hasPermission = user.Claims.Any(x =>
            string.Equals(x.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Value, _permissionCode, StringComparison.OrdinalIgnoreCase));

        return hasAdminScope && hasPermission;
    }
}
