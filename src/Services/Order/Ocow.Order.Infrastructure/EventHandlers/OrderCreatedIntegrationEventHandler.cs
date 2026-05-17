using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.Order.Infrastructure.Data;
using Ocow.Order.Infrastructure.Models;

namespace Ocow.Order.Infrastructure.EventHandlers;

/// <summary>
/// 订单创建集成事件处理器，用于演示订单服务消费端的幂等处理。
/// </summary>
public sealed class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private const string EventName = "ocow.orders.created";

    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;
    private readonly OrderDbContext _dbContext;

    /// <summary>
    /// 创建订单创建集成事件处理器。
    /// </summary>
    public OrderCreatedIntegrationEventHandler(
        ILogger<OrderCreatedIntegrationEventHandler> logger,
        OrderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// 幂等处理订单创建集成事件。
    /// </summary>
    public async Task HandleAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var alreadyProcessed = await _dbContext.ProcessedIntegrationEvents
            .AnyAsync(x => x.EventId == integrationEvent.Id, cancellationToken);
        if (alreadyProcessed)
        {
            _logger.LogInformation("订单创建集成事件 {EventId} 已处理，跳过重复消费。", integrationEvent.Id);
            return;
        }

        _dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
        {
            EventId = integrationEvent.Id,
            EventName = EventName,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
