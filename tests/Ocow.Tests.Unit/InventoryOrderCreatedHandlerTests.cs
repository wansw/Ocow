using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ocow.Contracts.Events.Inventory;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.Inventory.Domain.Models;
using Ocow.Inventory.Infrastructure.Data;
using Ocow.Inventory.Infrastructure.EventHandlers;
using Ocow.Inventory.Infrastructure.Models;

namespace Ocow.Tests.Unit;

/// <summary>
/// 库存订单创建事件处理器测试，用于验证订单创建后锁定库存并发布结果事件。
/// </summary>
public class InventoryOrderCreatedHandlerTests
{
    /// <summary>
    /// 验证订单创建事件会锁定库存并发布库存锁定成功事件。
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenStockIsEnough_ShouldLockInventoryAndPublishLockedEvent()
    {
        await using var dbContext = CreateDbContext();
        var productId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        dbContext.InventoryItems.Add(InventoryItem.Create(productId, skuId, 10));
        await dbContext.SaveChangesAsync();
        var eventBus = new CapturingEventBus();
        var executor = new InlineInventoryExecutor(dbContext, eventBus);
        var handler = new OrderCreatedIntegrationEventHandler(
            executor,
            NullLogger<OrderCreatedIntegrationEventHandler>.Instance);
        var integrationEvent = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            20,
            "CNY")
        {
            Items =
            [
                new OrderCreatedIntegrationEventItem(productId, skuId, 2)
            ]
        };

        await handler.HandleAsync(integrationEvent, CancellationToken.None);

        var inventory = await dbContext.InventoryItems.SingleAsync();
        Assert.Equal(8, inventory.AvailableQuantity);
        Assert.Equal(2, inventory.LockedQuantity);
        Assert.Single(dbContext.InventoryLocks);
        Assert.Contains(eventBus.PublishedEvents, x => x is InventoryLockedIntegrationEvent);
    }

    /// <summary>
    /// 验证库存不足时不会锁定库存并会发布库存锁定失败事件。
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenStockIsNotEnough_ShouldPublishLockFailedEvent()
    {
        await using var dbContext = CreateDbContext();
        var productId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        dbContext.InventoryItems.Add(InventoryItem.Create(productId, skuId, 1));
        await dbContext.SaveChangesAsync();
        var eventBus = new CapturingEventBus();
        var executor = new InlineInventoryExecutor(dbContext, eventBus);
        var handler = new OrderCreatedIntegrationEventHandler(
            executor,
            NullLogger<OrderCreatedIntegrationEventHandler>.Instance);
        var integrationEvent = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            20,
            "CNY")
        {
            Items =
            [
                new OrderCreatedIntegrationEventItem(productId, skuId, 2)
            ]
        };

        await handler.HandleAsync(integrationEvent, CancellationToken.None);

        var inventory = await dbContext.InventoryItems.SingleAsync();
        Assert.Equal(1, inventory.AvailableQuantity);
        Assert.Equal(0, inventory.LockedQuantity);
        Assert.Empty(dbContext.InventoryLocks);
        Assert.Contains(eventBus.PublishedEvents, x => x is InventoryLockFailedIntegrationEvent);
    }

    /// <summary>
    /// 创建库存测试数据库上下文。
    /// </summary>
    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new InventoryDbContext(options);
    }

    private sealed class InlineInventoryExecutor : ICapTransactionalExecutor<InventoryDbContext>
    {
        private readonly InventoryDbContext _dbContext;
        private readonly IEventBus _eventBus;

        public InlineInventoryExecutor(InventoryDbContext dbContext, IEventBus eventBus)
        {
            _dbContext = dbContext;
            _eventBus = eventBus;
        }

        public async Task ExecuteAsync(
            Func<InventoryDbContext, IEventBus, CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            await operation(_dbContext, _eventBus, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<TResult> ExecuteAsync<TResult>(
            Func<InventoryDbContext, IEventBus, CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            var result = await operation(_dbContext, _eventBus, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }
    }

    private sealed class CapturingEventBus : IEventBus
    {
        public List<object> PublishedEvents { get; } = new();

        public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : Ocow.Contracts.Abstractions.IntegrationEvent
        {
            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : Ocow.Contracts.Abstractions.IntegrationEvent
        {
            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task PublishDelayAsync<TEvent>(TimeSpan delay, TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : Ocow.Contracts.Abstractions.IntegrationEvent
        {
            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task PublishDelayAsync<TEvent>(TimeSpan delay, string eventName, TEvent integrationEvent, CancellationToken cancellationToken = default)
            where TEvent : Ocow.Contracts.Abstractions.IntegrationEvent
        {
            PublishedEvents.Add(integrationEvent);
            return Task.CompletedTask;
        }
    }
}
