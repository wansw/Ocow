namespace Ocow.MessageBus.Interfaces;

/// <summary>
/// 消息发布接口，用于向 RabbitMQ 发布跨服务事件。
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// 发布一条事件消息。
    /// </summary>
    Task PublishAsync<T>(string routingKey, T message, string traceId, CancellationToken cancellationToken = default);
}
