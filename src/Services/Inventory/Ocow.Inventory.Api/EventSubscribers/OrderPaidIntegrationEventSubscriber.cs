using DotNetCore.CAP;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.Inventory.Api.EventSubscribers;

/// <summary>
/// 订单支付成功 CAP 订阅适配器，用于确认扣减库存。
/// </summary>
public sealed class OrderPaidIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderPaidIntegrationEvent> _handler;

    /// <summary>
    /// 创建订单支付成功 CAP 订阅适配器。
    /// </summary>
    public OrderPaidIntegrationEventSubscriber(IIntegrationEventHandler<OrderPaidIntegrationEvent> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 接收订单支付成功事件。
    /// </summary>
    [CapSubscribe("ocow.orders.paid")]
    public Task HandleAsync(OrderPaidIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(integrationEvent, cancellationToken);
    }
}
