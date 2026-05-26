using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Orders;

/// <summary>
/// 订单取消集成事件，用于通知库存服务释放库存锁定。
/// </summary>
[IntegrationEventName("ocow.orders.canceled")]
public sealed record OrderCanceledIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
