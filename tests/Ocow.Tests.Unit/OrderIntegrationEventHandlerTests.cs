using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ocow.Contracts.Events.Orders;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.EventHandlers;
using Ocow.Order.Infrastructure.Models;

namespace Ocow.Tests.Unit;

/// <summary>
/// 订单集成事件处理器测试，用于验证消费者幂等记录逻辑。
/// </summary>
public class OrderIntegrationEventHandlerTests
{
    /// <summary>
    /// 验证首次处理订单创建事件时会写入已处理事件记录。
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenEventIsNew_ShouldRecordProcessedEvent()
    {
        await using var dbContext = CreateDbContext();
        var handler = new OrderCreatedIntegrationEventHandler(
            NullLogger<OrderCreatedIntegrationEventHandler>.Instance,
            dbContext);
        var integrationEvent = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            88,
            "CNY");

        await handler.HandleAsync(integrationEvent, CancellationToken.None);

        var processedEvent = await dbContext.ProcessedIntegrationEvents.SingleAsync();
        Assert.Equal(integrationEvent.Id, processedEvent.EventId);
        Assert.Equal("ocow.orders.created", processedEvent.EventName);
    }

    /// <summary>
    /// 验证重复处理同一订单创建事件时不会写入重复幂等记录。
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenEventAlreadyProcessed_ShouldSkipDuplicateRecord()
    {
        await using var dbContext = CreateDbContext();
        var integrationEvent = new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            88,
            "CNY");
        dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
        {
            EventId = integrationEvent.Id,
            EventName = "ocow.orders.created",
            ProcessedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var handler = new OrderCreatedIntegrationEventHandler(
            NullLogger<OrderCreatedIntegrationEventHandler>.Instance,
            dbContext);

        await handler.HandleAsync(integrationEvent, CancellationToken.None);

        Assert.Equal(1, await dbContext.ProcessedIntegrationEvents.CountAsync());
    }

    /// <summary>
    /// 创建订单测试数据库上下文。
    /// </summary>
    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new OrderDbContext(options);
    }
}
