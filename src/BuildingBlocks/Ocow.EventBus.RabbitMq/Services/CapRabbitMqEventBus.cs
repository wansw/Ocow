using DotNetCore.CAP;
using Ocow.Contracts.Abstractions;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.EventBus.RabbitMq.Services;

/// <summary>
/// 基于 CAP 的 RabbitMQ 事件总线实现。
/// </summary>
public sealed class CapRabbitMqEventBus : IEventBus
{
    private readonly ICapPublisher _capPublisher;
    private readonly IIntegrationEventNameProvider _eventNameProvider;

    /// <summary>
    /// 创建基于 CAP 的 RabbitMQ 事件总线。
    /// </summary>
    public CapRabbitMqEventBus(ICapPublisher capPublisher, IIntegrationEventNameProvider eventNameProvider)
    {
        _capPublisher = capPublisher;
        _eventNameProvider = eventNameProvider;
    }

    /// <summary>
    /// 按事件类型约定的事件名发布集成事件。
    /// </summary>
    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventName = _eventNameProvider.GetName<TEvent>();
        return PublishAsync(eventName, integrationEvent, cancellationToken);
    }

    /// <summary>
    /// 按显式事件名发布集成事件，并写入标准事件头。
    /// </summary>
    public Task PublishAsync<TEvent>(string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("事件名称不能为空。", nameof(eventName));
        }

        ArgumentNullException.ThrowIfNull(integrationEvent);

        var headers = new Dictionary<string, string?>
        {
            ["x-event-id"] = integrationEvent.Id.ToString("N"),
            ["x-event-name"] = eventName,
            ["x-event-type"] = integrationEvent.GetType().FullName,
            ["x-occurred-on-utc"] = integrationEvent.OccurredOnUtc.ToString("O"),
            ["x-correlation-id"] = integrationEvent.CorrelationId,
            ["x-causation-id"] = integrationEvent.CausationId
        };

        return _capPublisher.PublishAsync(
            name: eventName,
            contentObj: integrationEvent,
            headers: headers,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 按事件类型约定的事件名延迟发布集成事件。
    /// </summary>
    public Task PublishDelayAsync<TEvent>(TimeSpan delay, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventName = _eventNameProvider.GetName<TEvent>();
        return PublishDelayAsync(delay, eventName, integrationEvent, cancellationToken);
    }

    /// <summary>
    /// 按显式事件名延迟发布集成事件，并写入标准事件头。
    /// </summary>
    public Task PublishDelayAsync<TEvent>(TimeSpan delay, string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (delay <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "延迟时间必须大于 0。");
        }

        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("事件名称不能为空。", nameof(eventName));
        }

        ArgumentNullException.ThrowIfNull(integrationEvent);

        var headers = new Dictionary<string, string?>
        {
            ["x-event-id"] = integrationEvent.Id.ToString("N"),
            ["x-event-name"] = eventName,
            ["x-event-type"] = integrationEvent.GetType().FullName,
            ["x-occurred-on-utc"] = integrationEvent.OccurredOnUtc.ToString("O"),
            ["x-correlation-id"] = integrationEvent.CorrelationId,
            ["x-causation-id"] = integrationEvent.CausationId
        };

        return _capPublisher.PublishDelayAsync(
            delay,
            eventName,
            integrationEvent,
            headers,
            cancellationToken);
    }
}
