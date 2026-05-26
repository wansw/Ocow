using DotNetCore.CAP;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.Order.Api.EventSubscribers;

/// <summary>
/// 订单支付超时 CAP 订阅适配器，用于触发订单自动取消。
/// </summary>
public sealed class OrderPaymentTimeoutIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderPaymentTimeoutIntegrationEvent> _handler;

    /// <summary>
    /// 创建订单支付超时 CAP 订阅适配器。
    /// </summary>
    public OrderPaymentTimeoutIntegrationEventSubscriber(IIntegrationEventHandler<OrderPaymentTimeoutIntegrationEvent> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 接收订单支付超时事件。
    /// </summary>
    [CapSubscribe("ocow.orders.payment-timeout")]
    public Task HandleAsync(OrderPaymentTimeoutIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(integrationEvent, cancellationToken);
    }
}
