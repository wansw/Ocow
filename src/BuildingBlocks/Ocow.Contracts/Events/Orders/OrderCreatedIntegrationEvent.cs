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
    string Currency) : IntegrationEvent
{
    /// <summary>
    /// 订单商品明细，用于库存服务锁定库存。
    /// </summary>
    public IReadOnlyList<OrderCreatedIntegrationEventItem> Items { get; init; } = [];
}

/// <summary>
/// 订单创建事件商品明细，用于传递库存锁定所需的商品和数量。
/// </summary>
public sealed record OrderCreatedIntegrationEventItem(
    Guid ProductId,
    Guid SkuId,
    int Quantity);
