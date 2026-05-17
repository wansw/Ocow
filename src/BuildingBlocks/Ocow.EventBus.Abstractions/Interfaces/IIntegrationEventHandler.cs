using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions.Interfaces;

/// <summary>
/// 集成事件处理器抽象，用于隔离业务消费逻辑和具体消息中间件。
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    /// <summary>
    /// 处理收到的集成事件。
    /// </summary>
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken = default);
}
