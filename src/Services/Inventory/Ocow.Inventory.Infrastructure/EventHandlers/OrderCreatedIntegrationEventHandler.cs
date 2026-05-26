using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.Contracts.Events.Inventory;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Inventory.Domain.Models;
using Ocow.Inventory.Infrastructure.Data;
using Ocow.Inventory.Infrastructure.Models;

namespace Ocow.Inventory.Infrastructure.EventHandlers;

/// <summary>
/// 订单创建集成事件处理器，用于在库存服务中锁定订单所需库存。
/// </summary>
public sealed class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private const string EventName = "ocow.orders.created";
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(15);

    private readonly ICapTransactionalExecutor<InventoryDbContext> _transactionalExecutor;
    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

    /// <summary>
    /// 创建订单创建集成事件处理器。
    /// </summary>
    public OrderCreatedIntegrationEventHandler(
        ICapTransactionalExecutor<InventoryDbContext> transactionalExecutor,
        ILogger<OrderCreatedIntegrationEventHandler> logger)
    {
        _transactionalExecutor = transactionalExecutor;
        _logger = logger;
    }

    /// <summary>
    /// 处理订单创建事件，库存足够时锁定库存，不足时发布锁定失败事件。
    /// </summary>
    public Task HandleAsync(OrderCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        return _transactionalExecutor.ExecuteAsync(async (dbContext, eventBus, ct) =>
        {
            if (await IsProcessedAsync(dbContext, integrationEvent.Id, ct))
            {
                _logger.LogInformation("订单创建事件 {EventId} 已处理，跳过重复库存锁定。", integrationEvent.Id);
                return;
            }

            if (await dbContext.InventoryLocks.AnyAsync(x => x.OrderId == integrationEvent.OrderId, ct))
            {
                AddProcessedEvent(dbContext, integrationEvent.Id, EventName);
                return;
            }

            var requestedItems = integrationEvent.Items;
            if (requestedItems.Count == 0)
            {
                AddProcessedEvent(dbContext, integrationEvent.Id, EventName);
                await eventBus.PublishAsync(
                    new InventoryLockFailedIntegrationEvent(integrationEvent.OrderId, "订单事件缺少库存锁定明细。")
                    {
                        CorrelationId = integrationEvent.CorrelationId,
                        CausationId = integrationEvent.Id.ToString("N")
                    },
                    ct);
                return;
            }

            var skuIds = requestedItems.Select(x => x.SkuId).Distinct().ToArray();
            var inventoryItems = await dbContext.InventoryItems
                .Where(x => skuIds.Contains(x.SkuId))
                .ToDictionaryAsync(x => x.SkuId, ct);

            foreach (var requestedItem in requestedItems)
            {
                if (!inventoryItems.TryGetValue(requestedItem.SkuId, out var inventoryItem) ||
                    inventoryItem.AvailableQuantity < requestedItem.Quantity)
                {
                    AddProcessedEvent(dbContext, integrationEvent.Id, EventName);
                    await eventBus.PublishAsync(
                        new InventoryLockFailedIntegrationEvent(integrationEvent.OrderId, "库存不足。")
                        {
                            CorrelationId = integrationEvent.CorrelationId,
                            CausationId = integrationEvent.Id.ToString("N")
                        },
                        ct);
                    return;
                }
            }

            var lockItems = new List<InventoryLockItem>();
            foreach (var requestedItem in requestedItems)
            {
                var inventoryItem = inventoryItems[requestedItem.SkuId];
                inventoryItem.Lock(requestedItem.Quantity);
                lockItems.Add(InventoryLockItem.Create(
                    requestedItem.ProductId,
                    requestedItem.SkuId,
                    requestedItem.Quantity));
            }

            var inventoryLock = InventoryLock.Create(
                integrationEvent.OrderId,
                DateTime.UtcNow.Add(LockTimeout),
                lockItems);
            dbContext.InventoryLocks.Add(inventoryLock);
            AddProcessedEvent(dbContext, integrationEvent.Id, EventName);

            await eventBus.PublishAsync(
                new InventoryLockedIntegrationEvent(integrationEvent.OrderId, inventoryLock.Id)
                {
                    CorrelationId = integrationEvent.CorrelationId,
                    CausationId = integrationEvent.Id.ToString("N")
                },
                ct);
        }, cancellationToken);
    }

    /// <summary>
    /// 判断集成事件是否已经处理。
    /// </summary>
    private static Task<bool> IsProcessedAsync(InventoryDbContext dbContext, Guid eventId, CancellationToken cancellationToken)
    {
        return dbContext.ProcessedIntegrationEvents.AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

    /// <summary>
    /// 记录已处理集成事件。
    /// </summary>
    private static void AddProcessedEvent(InventoryDbContext dbContext, Guid eventId, string eventName)
    {
        dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
        {
            EventId = eventId,
            EventName = eventName,
            ProcessedAtUtc = DateTime.UtcNow
        });
    }
}
