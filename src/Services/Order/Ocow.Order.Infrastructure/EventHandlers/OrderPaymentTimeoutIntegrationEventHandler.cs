using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Order.Domain.Enums;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Models;

namespace Ocow.Order.Infrastructure.EventHandlers;

/// <summary>
/// 订单支付超时事件处理器，用于自动取消仍处于待支付状态的订单。
/// </summary>
public sealed class OrderPaymentTimeoutIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentTimeoutIntegrationEvent>
{
    private const string EventName = "ocow.orders.payment-timeout";

    private readonly ICapTransactionalExecutor<OrderDbContext> _transactionalExecutor;
    private readonly ILogger<OrderPaymentTimeoutIntegrationEventHandler> _logger;

    /// <summary>
    /// 创建订单支付超时事件处理器。
    /// </summary>
    public OrderPaymentTimeoutIntegrationEventHandler(
        ICapTransactionalExecutor<OrderDbContext> transactionalExecutor,
        ILogger<OrderPaymentTimeoutIntegrationEventHandler> logger)
    {
        _transactionalExecutor = transactionalExecutor;
        _logger = logger;
    }

    /// <summary>
    /// 处理支付超时事件并发布订单取消事件。
    /// </summary>
    public Task HandleAsync(OrderPaymentTimeoutIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return _transactionalExecutor.ExecuteAsync(async (dbContext, eventBus, ct) =>
        {
            if (await dbContext.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == integrationEvent.Id, ct))
            {
                _logger.LogInformation("订单支付超时事件 {EventId} 已处理，跳过重复取消。", integrationEvent.Id);
                return;
            }

            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == integrationEvent.OrderId, ct);
            if (order is not null && order.Status == OrderStatusEnum.PendingPay)
            {
                order.Cancel();
                await eventBus.PublishAsync(
                    new OrderCanceledIntegrationEvent(order.Id, "订单支付超时自动取消。")
                    {
                        CorrelationId = integrationEvent.CorrelationId,
                        CausationId = integrationEvent.Id.ToString("N")
                    },
                    ct);
            }

            dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
            {
                EventId = integrationEvent.Id,
                EventName = EventName,
                ProcessedAtUtc = DateTime.UtcNow
            });
        }, cancellationToken);
    }
}
