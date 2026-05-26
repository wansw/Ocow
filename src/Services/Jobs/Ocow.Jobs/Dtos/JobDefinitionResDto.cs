namespace Ocow.Jobs.Api.Dtos;

/// <summary>
/// 后台任务定义响应 DTO，用于返回动态任务配置结果。
/// </summary>
public class JobDefinitionResDto
{
    /// <summary>
    /// 任务主键。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 任务编码。
    /// </summary>
    public string JobCode { get; init; } = string.Empty;

    /// <summary>
    /// 任务名称。
    /// </summary>
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// 任务类型。
    /// </summary>
    public string JobType { get; init; } = string.Empty;

    /// <summary>
    /// Cron 表达式。
    /// </summary>
    public string Cron { get; init; } = string.Empty;

    /// <summary>
    /// 是否启用任务。
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 目标服务。
    /// </summary>
    public string TargetService { get; init; } = string.Empty;

    /// <summary>
    /// 目标接口地址。
    /// </summary>
    public string TargetApi { get; init; } = string.Empty;

    /// <summary>
    /// HTTP 方法。
    /// </summary>
    public string HttpMethod { get; init; } = "POST";
}
