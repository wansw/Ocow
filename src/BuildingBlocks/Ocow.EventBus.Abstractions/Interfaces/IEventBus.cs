using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions.Interfaces;

/// <summary>
/// 事件总线抽象，用于发布跨服务集成事件。
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 按事件类型约定的事件名发布集成事件。
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// 按显式事件名发布集成事件。
    /// </summary>
    Task PublishAsync<TEvent>(string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// 按事件类型约定的事件名延迟发布集成事件。
    /// </summary>
    Task PublishDelayAsync<TEvent>(TimeSpan delay, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// 按显式事件名延迟发布集成事件。
    /// </summary>
    Task PublishDelayAsync<TEvent>(TimeSpan delay, string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
