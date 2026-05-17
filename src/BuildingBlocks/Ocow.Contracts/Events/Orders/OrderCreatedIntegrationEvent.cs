using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Orders;

/// <summary>
/// 订单创建集成事件，用于通知其他服务订单已经创建。
/// </summary>
[IntegrationEventName("ocow.orders.created")]
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string Currency) : IntegrationEvent;
