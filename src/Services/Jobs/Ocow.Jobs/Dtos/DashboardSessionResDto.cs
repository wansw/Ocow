namespace Ocow.Jobs.Api.Dtos;

/// <summary>
/// Dashboard 会话响应 DTO，用于返回 Hangfire Dashboard 入口和会话过期时间。
/// </summary>
public class DashboardSessionResDto
{
    /// <summary>
    /// Hangfire Dashboard 访问路径。
    /// </summary>
    public string DashboardPath { get; init; } = string.Empty;

    /// <summary>
    /// Dashboard 浏览器会话过期时间。
    /// </summary>
    public DateTimeOffset ExpiresAt { get; init; }
}
