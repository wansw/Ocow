using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Jobs.Api.Models;

/// <summary>
/// 任务执行日志表实体，用于记录后台任务每次执行结果。
/// </summary>
[Table("job_execution_logs")]
public class JobExecutionLog
{
    /// <summary>
    /// 执行日志主键。
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 任务配置主键。
    /// </summary>
    public Guid JobDefinitionId { get; set; }

    /// <summary>
    /// 任务编码。
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string JobCode { get; set; } = string.Empty;

    /// <summary>
    /// 执行开始时间。
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 执行结束时间。
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// 是否执行成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP 状态码。
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// 执行消息。
    /// </summary>
    [MaxLength(512)]
    public string? Message { get; set; }

    /// <summary>
    /// 响应内容。
    /// </summary>
    public string? ResponseBody { get; set; }
}
