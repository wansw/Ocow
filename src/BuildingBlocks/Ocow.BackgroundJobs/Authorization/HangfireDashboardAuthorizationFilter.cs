using Hangfire.Dashboard;

namespace Ocow.BackgroundJobs.Authorization;

/// <summary>
/// Hangfire Dashboard 授权过滤器，用于把 Dashboard 访问控制接入 Ocow 后台管理员权限。
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly HangfireDashboardAdminAuthorizer _authorizer;

    /// <summary>
    /// 创建使用默认权限点的 Hangfire Dashboard 授权过滤器。
    /// </summary>
    public HangfireDashboardAuthorizationFilter()
        : this(new HangfireDashboardAdminAuthorizer())
    {
    }

    /// <summary>
    /// 创建指定权限点的 Hangfire Dashboard 授权过滤器。
    /// </summary>
    public HangfireDashboardAuthorizationFilter(string permissionCode)
        : this(new HangfireDashboardAdminAuthorizer(permissionCode))
    {
    }

    /// <summary>
    /// 创建指定授权器的 Hangfire Dashboard 授权过滤器。
    /// </summary>
    public HangfireDashboardAuthorizationFilter(HangfireDashboardAdminAuthorizer authorizer)
    {
        _authorizer = authorizer;
    }

    /// <summary>
    /// 校验当前 Dashboard 请求是否允许访问。
    /// </summary>
    public bool Authorize(DashboardContext context)
    {
        return _authorizer.IsAuthorized(context.GetHttpContext().User);
    }
}
