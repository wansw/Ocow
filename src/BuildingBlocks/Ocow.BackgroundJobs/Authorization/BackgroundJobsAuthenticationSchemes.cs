namespace Ocow.BackgroundJobs.Authorization;

/// <summary>
/// 后台任务认证方案常量，用于区分 Hangfire Dashboard 浏览器会话。
/// </summary>
public static class BackgroundJobsAuthenticationSchemes
{
    /// <summary>
    /// Hangfire Dashboard Cookie 认证方案名称。
    /// </summary>
    public const string DashboardCookie = "HangfireDashboardCookie";
}
