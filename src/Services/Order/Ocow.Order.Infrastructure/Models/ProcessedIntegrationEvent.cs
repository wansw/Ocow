using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Order.Infrastructure.Models;

/// <summary>
/// 已处理集成事件实体，用于保障订单服务消费者幂等。
/// </summary>
[Table("processed_integration_events")]
public class ProcessedIntegrationEvent
{
    /// <summary>
    /// 集成事件编号。
    /// </summary>
    [Key]
    public Guid EventId { get; set; }

    /// <summary>
    /// 集成事件名称。
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// 事件处理完成的 UTC 时间。
    /// </summary>
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
