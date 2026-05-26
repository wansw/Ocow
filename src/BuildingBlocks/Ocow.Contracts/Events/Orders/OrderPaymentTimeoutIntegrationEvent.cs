using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Orders;

/// <summary>
/// 订单支付超时集成事件，用于触发待支付订单自动取消。
/// </summary>
[IntegrationEventName("ocow.orders.payment-timeout")]
public sealed record OrderPaymentTimeoutIntegrationEvent(Guid OrderId) : IntegrationEvent;
