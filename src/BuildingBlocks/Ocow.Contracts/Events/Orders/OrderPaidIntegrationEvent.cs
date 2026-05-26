using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Orders;

/// <summary>
/// 订单支付成功集成事件，用于通知库存服务确认扣减库存。
/// </summary>
[IntegrationEventName("ocow.orders.paid")]
public sealed record OrderPaidIntegrationEvent(Guid OrderId) : IntegrationEvent;
