using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Application.Interfaces;

/// <summary>
/// 订单取消事务边界，用于在同一事务中取消订单并发布订单取消事件。
/// </summary>
public interface IOrderCancellationTransaction
{
    /// <summary>
    /// 取消待支付订单并发布订单取消集成事件。
    /// </summary>
    Task<OrderEntity> CancelAsync(Guid orderId, string reason, CancellationToken cancellationToken = default);
}
