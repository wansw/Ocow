using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Application.Interfaces;

/// <summary>
/// 订单创建事务边界，用于在同一事务中保存订单并发布订单创建事件。
/// </summary>
public interface IOrderCreationTransaction
{
    /// <summary>
    /// 保存订单并发布订单创建集成事件。
    /// </summary>
    Task<OrderEntity> CreateAsync(OrderEntity order, CancellationToken cancellationToken = default);
}
