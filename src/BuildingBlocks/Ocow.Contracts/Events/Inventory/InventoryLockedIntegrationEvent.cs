using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Inventory;

/// <summary>
/// 库存锁定成功集成事件，用于通知订单服务库存已经预占。
/// </summary>
[IntegrationEventName("ocow.inventory.locked")]
public sealed record InventoryLockedIntegrationEvent(Guid OrderId, Guid InventoryLockId) : IntegrationEvent;
