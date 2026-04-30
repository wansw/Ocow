namespace Ocow.Contracts;

/// <summary>
/// 订单创建集成事件，用于通知库存、营销、统计等服务。
/// </summary>
public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public Guid CustomerId { get; init; }

    public decimal TotalAmount { get; init; }
}
