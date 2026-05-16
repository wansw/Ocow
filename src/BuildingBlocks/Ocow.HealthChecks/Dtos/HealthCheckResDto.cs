namespace Ocow.HealthChecks.Dtos;

/// <summary>
/// 健康检查响应 DTO，用于返回服务名称、健康状态和检查明细。
/// </summary>
public class HealthCheckResDto
{
    /// <summary>
    /// 服务名称。
    /// </summary>
    public string Service { get; init; } = string.Empty;

    /// <summary>
    /// 健康状态。
    /// </summary>
    public string Status { get; init; } = "ok";

    /// <summary>
    /// 当前请求 TraceId。
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// 检查耗时。
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// 检查时间。
    /// </summary>
    public DateTime CheckedAt { get; init; }

    /// <summary>
    /// 健康检查项明细。
    /// </summary>
    public IReadOnlyDictionary<string, string> Entries { get; init; } = new Dictionary<string, string>();
}
