namespace Ocow.Contracts;

/// <summary>
/// 集成事件基类，用于跨服务 RabbitMQ 消息契约。
/// </summary>
public abstract class IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public string TraceId { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public int Version { get; init; } = 1;
}
