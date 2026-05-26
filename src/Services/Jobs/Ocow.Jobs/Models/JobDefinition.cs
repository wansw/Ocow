using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Jobs.Api.Models;

/// <summary>
/// 任务配置表实体，用于保存后台动态任务定义。
/// </summary>
[Table("job_definitions")]
public class JobDefinition
{
    /// <summary>
    /// 任务配置主键。
    /// </summary>
    [Key]
    public Guid Id { get; set; }


    /// <summary>
    /// 任务名称。
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型。
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string JobType { get; set; } = "Http";

    /// <summary>
    /// Cron 表达式。
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Cron { get; set; } = "*/5 * * * *";

    /// <summary>
    /// 目标服务名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string TargetService { get; set; } = string.Empty;

    /// <summary>
    /// 目标接口地址。
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string TargetApi { get; set; } = string.Empty;

    /// <summary>
    /// HTTP 方法。
    /// </summary>
    [Required]
    [MaxLength(16)]
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// 请求体 JSON。
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 是否启用任务。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
