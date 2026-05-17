using DotNetCore.CAP;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.Order.Api.EventSubscribers;

/// <summary>
/// 订单创建 CAP 订阅适配器，用于把 CAP 消息转交给业务事件处理器。
/// </summary>
public sealed class OrderCreatedIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderCreatedIntegrationEvent> _handler;

    /// <summary>
    /// 创建订单创建 CAP 订阅适配器。
    /// </summary>
    public OrderCreatedIntegrationEventSubscriber(IIntegrationEventHandler<OrderCreatedIntegrationEvent> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 接收订单创建集成事件并调用业务处理器。
    /// </summary>
    [CapSubscribe("ocow.orders.created")]
    public Task HandleAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(integrationEvent, cancellationToken);
    }
}
