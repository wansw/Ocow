using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.Contracts.Events.Inventory;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Order.Domain.Enums;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Models;

namespace Ocow.Order.Infrastructure.EventHandlers;

/// <summary>
/// 库存锁定失败事件处理器，用于取消库存不足的订单。
/// </summary>
public sealed class InventoryLockFailedIntegrationEventHandler : IIntegrationEventHandler<InventoryLockFailedIntegrationEvent>
{
    private const string EventName = "ocow.inventory.lock-failed";

    private readonly ICapTransactionalExecutor<OrderDbContext> _transactionalExecutor;
    private readonly ILogger<InventoryLockFailedIntegrationEventHandler> _logger;

    /// <summary>
    /// 创建库存锁定失败事件处理器。
    /// </summary>
    public InventoryLockFailedIntegrationEventHandler(
        ICapTransactionalExecutor<OrderDbContext> transactionalExecutor,
        ILogger<InventoryLockFailedIntegrationEventHandler> logger)
    {
        _transactionalExecutor = transactionalExecutor;
        _logger = logger;
    }

    /// <summary>
    /// 处理库存锁定失败事件并发布订单取消事件。
    /// </summary>
    public Task HandleAsync(InventoryLockFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return _transactionalExecutor.ExecuteAsync(async (dbContext, eventBus, ct) =>
        {
            if (await dbContext.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == integrationEvent.Id, ct))
            {
                _logger.LogInformation("库存锁定失败事件 {EventId} 已处理，跳过重复取消。", integrationEvent.Id);
                return;
            }

            var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == integrationEvent.OrderId, ct);
            if (order is not null && order.Status == OrderStatusEnum.PendingPay)
            {
                order.Cancel();
                await eventBus.PublishAsync(
                    new OrderCanceledIntegrationEvent(order.Id, integrationEvent.Reason)
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
