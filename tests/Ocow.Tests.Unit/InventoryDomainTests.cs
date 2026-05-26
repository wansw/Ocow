using Ocow.Inventory.Domain.Enums;
using Ocow.Inventory.Domain.Models;

namespace Ocow.Tests.Unit;

/// <summary>
/// 库存领域测试，用于验证库存锁定、释放和确认扣减规则。
/// </summary>
public class InventoryDomainTests
{
    /// <summary>
    /// 验证锁定库存时会减少可售库存并增加锁定库存。
    /// </summary>
    [Fact]
    public void Lock_ShouldMoveAvailableQuantityToLockedQuantity()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10);

        inventory.Lock(3);

        Assert.Equal(7, inventory.AvailableQuantity);
        Assert.Equal(3, inventory.LockedQuantity);
    }

    /// <summary>
    /// 验证释放锁定库存时会把锁定库存还回可售库存。
    /// </summary>
    [Fact]
    public void Release_ShouldMoveLockedQuantityBackToAvailableQuantity()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10);
        inventory.Lock(3);

        inventory.Release(3);

        Assert.Equal(10, inventory.AvailableQuantity);
        Assert.Equal(0, inventory.LockedQuantity);
    }

    /// <summary>
    /// 验证确认扣减库存时会减少锁定库存且不回补可售库存。
    /// </summary>
    [Fact]
    public void ConfirmDeduction_ShouldReduceLockedQuantity()
    {
        var inventory = InventoryItem.Create(Guid.NewGuid(), Guid.NewGuid(), 10);
        inventory.Lock(3);

        inventory.ConfirmDeduction(3);

        Assert.Equal(7, inventory.AvailableQuantity);
        Assert.Equal(0, inventory.LockedQuantity);
    }

    /// <summary>
    /// 验证库存锁释放后状态会变更为已释放。
    /// </summary>
    [Fact]
    public void InventoryLock_Release_ShouldChangeStatus()
    {
        var inventoryLock = InventoryLock.Create(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(15), []);

        inventoryLock.Release();

        Assert.Equal(InventoryLockStatusEnum.Released, inventoryLock.Status);
    }
}
