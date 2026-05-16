using Ocow.BackgroundJobs.Authorization;

namespace Ocow.BackgroundJobs.Options;

/// <summary>
/// 后台任务配置实体，用于绑定 Hangfire Dashboard 路径、权限点和持久化存储配置。
/// </summary>
public class BackgroundJobsOption
{
    /// <summary>
    /// Hangfire Dashboard 访问路径。
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Hangfire Dashboard 页面标题。
    /// </summary>
    public string DashboardTitle { get; set; } = "Ocow Background Jobs";

    /// <summary>
    /// 访问 Hangfire Dashboard 需要具备的后台权限点编码。
    /// </summary>
    public string DashboardPermissionCode { get; set; } = HangfireDashboardAdminAuthorizer.DefaultPermissionCode;

    /// <summary>
    /// Hangfire PostgreSQL 存储连接字符串。
    /// </summary>
    public string StorageConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Hangfire PostgreSQL 存储 Schema 名称。
    /// </summary>
    public string SchemaName { get; set; } = "hangfire";
}
