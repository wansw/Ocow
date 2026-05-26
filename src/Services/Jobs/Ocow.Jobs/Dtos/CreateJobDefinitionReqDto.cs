using System.ComponentModel.DataAnnotations;

namespace Ocow.Jobs.Api.Dtos;

/// <summary>
/// 创建或更新任务配置请求 DTO。
/// </summary>
public class CreateJobDefinitionReqDto
{

    /// <summary>
    /// 任务名称。
    /// </summary>
    [Required(ErrorMessage = "任务名称不能为空")]
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// 任务类型，当前默认使用 Http。
    /// </summary>
    public string JobType { get; init; } = "Http";

    /// <summary>
    /// Cron 表达式。
    /// </summary>
    public string Cron { get; init; } = "*/1 * * * *";

    /// <summary>
    /// 目标服务名称。
    /// </summary>
    public string TargetService { get; init; } = string.Empty;

    /// <summary>
    /// 目标接口地址。
    /// </summary>
    [Required(ErrorMessage = "目标接口不能为空")]
    public string TargetApi { get; init; } = string.Empty;

    /// <summary>
    /// HTTP 方法。
    /// </summary>
    public string HttpMethod { get; init; } = "POST";

    /// <summary>
    /// 请求体 JSON。
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// 是否启用任务。
    /// </summary>
    public bool Enabled { get; init; } = true;
}
