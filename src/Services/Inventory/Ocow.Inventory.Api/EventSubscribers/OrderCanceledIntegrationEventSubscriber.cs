using DotNetCore.CAP;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.Inventory.Api.EventSubscribers;

/// <summary>
/// 订单取消 CAP 订阅适配器，用于释放库存锁。
/// </summary>
public sealed class OrderCanceledIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderCanceledIntegrationEvent> _handler;

    /// <summary>
    /// 创建订单取消 CAP 订阅适配器。
    /// </summary>
    public OrderCanceledIntegrationEventSubscriber(IIntegrationEventHandler<OrderCanceledIntegrationEvent> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 接收订单取消事件。
    /// </summary>
    [CapSubscribe("ocow.orders.canceled")]
    public Task HandleAsync(OrderCanceledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(integrationEvent, cancellationToken);
    }
}
