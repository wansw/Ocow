namespace Ocow.Jobs.Api.Dtos;

/// <summary>
/// 后台任务入队响应 DTO，用于返回 Hangfire 任务编号和 Dashboard 入口。
/// </summary>
public class EnqueueJobResDto
{
    /// <summary>
    /// Hangfire 任务编号。
    /// </summary>
    public string JobId { get; init; } = string.Empty;

    /// <summary>
    /// 任务名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Hangfire Dashboard 访问路径。
    /// </summary>
    public string DashboardPath { get; init; } = string.Empty;
}
