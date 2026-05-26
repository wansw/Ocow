namespace Ocow.Inventory.Domain.Enums;

/// <summary>
/// 库存锁状态枚举，用于表达库存预占、释放和确认扣减生命周期。
/// </summary>
public enum InventoryLockStatusEnum
{
    Locked = 1,
    Released = 2,
    Confirmed = 3
}
