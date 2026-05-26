using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Inventory.Infrastructure.Models;

/// <summary>
/// 已处理集成事件实体，用于保障库存服务消费者幂等。
/// </summary>
[Table("processed_integration_events")]
public class ProcessedIntegrationEvent
{
    [Key]
    public Guid EventId { get; set; }

    [Required]
    [MaxLength(256)]
    public string EventName { get; set; } = string.Empty;

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
