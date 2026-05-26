using DotNetCore.CAP;
using Ocow.Contracts.Events.Inventory;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.Order.Api.EventSubscribers;

/// <summary>
/// 库存锁定失败 CAP 订阅适配器，用于触发订单取消。
/// </summary>
public sealed class InventoryLockFailedIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<InventoryLockFailedIntegrationEvent> _handler;

    /// <summary>
    /// 创建库存锁定失败 CAP 订阅适配器。
    /// </summary>
    public InventoryLockFailedIntegrationEventSubscriber(IIntegrationEventHandler<InventoryLockFailedIntegrationEvent> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 接收库存锁定失败事件。
    /// </summary>
    [CapSubscribe("ocow.inventory.lock-failed")]
    public Task HandleAsync(InventoryLockFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return _handler.HandleAsync(integrationEvent, cancellationToken);
    }
}
