using Microsoft.EntityFrameworkCore;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Domain.Enums;
using Ocow.Order.Infrastructure.Data;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Infrastructure.Services;

/// <summary>
/// 基于 CAP 事务执行器的订单取消事务边界。
/// </summary>
public sealed class CapOrderCancellationTransaction : IOrderCancellationTransaction
{
    private readonly ICapTransactionalExecutor<OrderDbContext> _transactionalExecutor;

    /// <summary>
    /// 创建基于 CAP 的订单取消事务边界。
    /// </summary>
    public CapOrderCancellationTransaction(ICapTransactionalExecutor<OrderDbContext> transactionalExecutor)
    {
        _transactionalExecutor = transactionalExecutor;
    }

    /// <summary>
    /// 在同一 CAP 事务中取消订单并发布订单取消事件。
    /// </summary>
    public Task<OrderEntity> CancelAsync(Guid orderId, string reason, CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("订单编号不能为空。", nameof(orderId));
        }

        return _transactionalExecutor.ExecuteAsync(async (dbContext, eventBus, ct) =>
        {
            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct) ??
                        throw new InvalidOperationException("订单不存在。");

            if (order.Status == OrderStatusEnum.PendingPay)
            {
                order.Cancel();
                await eventBus.PublishAsync(
                    new OrderCanceledIntegrationEvent(order.Id, reason)
                    {
                        CorrelationId = order.Id.ToString("N")
                    },
                    ct);
            }

            return order;
        }, cancellationToken);
    }
}
