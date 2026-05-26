using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Inventory.Domain.Enums;
using Ocow.Inventory.Infrastructure.Data;
using Ocow.Inventory.Infrastructure.Models;

namespace Ocow.Inventory.Infrastructure.EventHandlers;

/// <summary>
/// 订单支付成功集成事件处理器，用于确认库存扣减。
/// </summary>
public sealed class OrderPaidIntegrationEventHandler : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    private const string EventName = "ocow.orders.paid";

    private readonly ICapTransactionalExecutor<InventoryDbContext> _transactionalExecutor;
    private readonly ILogger<OrderPaidIntegrationEventHandler> _logger;

    /// <summary>
    /// 创建订单支付成功集成事件处理器。
    /// </summary>
    public OrderPaidIntegrationEventHandler(
        ICapTransactionalExecutor<InventoryDbContext> transactionalExecutor,
        ILogger<OrderPaidIntegrationEventHandler> logger)
    {
        _transactionalExecutor = transactionalExecutor;
        _logger = logger;
    }

    /// <summary>
    /// 处理订单支付成功事件并确认扣减库存。
    /// </summary>
    public Task HandleAsync(OrderPaidIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return _transactionalExecutor.ExecuteAsync(async (dbContext, _, ct) =>
        {
            if (await dbContext.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == integrationEvent.Id, ct))
            {
                _logger.LogInformation("订单支付事件 {EventId} 已处理，跳过重复确认扣减。", integrationEvent.Id);
                return;
            }

            var inventoryLock = await dbContext.InventoryLocks
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.OrderId == integrationEvent.OrderId, ct);

            if (inventoryLock is not null && inventoryLock.Status == InventoryLockStatusEnum.Locked)
            {
                foreach (var lockItem in inventoryLock.Items)
                {
                    var inventoryItem = await dbContext.InventoryItems
                        .FirstAsync(x => x.SkuId == lockItem.SkuId, ct);
                    inventoryItem.ConfirmDeduction(lockItem.Quantity);
                }

                inventoryLock.Confirm();
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
