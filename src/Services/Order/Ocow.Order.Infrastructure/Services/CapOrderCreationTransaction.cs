using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Infrastructure.Data;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Infrastructure.Services;

/// <summary>
/// 基于 CAP 事务执行器的订单创建事务边界。
/// </summary>
public sealed class CapOrderCreationTransaction : IOrderCreationTransaction
{
    private const string DefaultCurrency = "CNY";

    private readonly ICapTransactionalExecutor<OrderDbContext> _transactionalExecutor;

    /// <summary>
    /// 创建基于 CAP 的订单创建事务边界。
    /// </summary>
    public CapOrderCreationTransaction(ICapTransactionalExecutor<OrderDbContext> transactionalExecutor)
    {
        _transactionalExecutor = transactionalExecutor;
    }

    /// <summary>
    /// 在同一 CAP 事务中保存订单并发布订单创建集成事件。
    /// </summary>
    public Task<OrderEntity> CreateAsync(OrderEntity order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);

        return _transactionalExecutor.ExecuteAsync(
            async (dbContext, eventBus, ct) =>
            {
                dbContext.Orders.Add(order);

                await eventBus.PublishAsync(
                    new OrderCreatedIntegrationEvent(
                        order.Id,
                        order.CustomerId,
                        order.TotalAmount,
                        DefaultCurrency)
                    {
                        CorrelationId = order.Id.ToString("N")
                    },
                    ct);

                return order;
            },
            cancellationToken);
    }
}
