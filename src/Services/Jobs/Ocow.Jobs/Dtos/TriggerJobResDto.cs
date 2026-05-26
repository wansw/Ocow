namespace Ocow.Jobs.Api.Dtos;

/// <summary>
/// 手动触发任务响应 DTO，用于返回 Hangfire 后台任务编号。
/// </summary>
public class TriggerJobResDto
{
    /// <summary>
    /// 任务编码。
    /// </summary>
    public Guid id { get; init; } 

    /// <summary>
    /// Hangfire 后台任务编号。
    /// </summary>
    public string BackgroundJobId { get; init; } = string.Empty;
}
