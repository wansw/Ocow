using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Inventory;

/// <summary>
/// 库存锁定失败集成事件，用于通知订单服务取消库存不足的订单。
/// </summary>
[IntegrationEventName("ocow.inventory.lock-failed")]
public sealed record InventoryLockFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
