namespace Ocow.MessageBus.Models;

/// <summary>
/// 消息信封模型，用于统一携带事件类型、业务负载和 TraceId。
/// </summary>
public class MessageEnvelope<T>
{
    public string EventType { get; init; } = typeof(T).Name;

    public string TraceId { get; init; } = string.Empty;

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public T? Payload { get; init; }
}
