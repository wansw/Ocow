using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocow.Inventory.Domain.Enums;

namespace Ocow.Inventory.Domain.Models;

/// <summary>
/// 库存锁实体，用于记录订单库存预占和后续释放或确认扣减。
/// </summary>
[Table("inventory_locks")]
public class InventoryLock
{
    [Key]
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public InventoryLockStatusEnum Status { get; private set; } = InventoryLockStatusEnum.Locked;

    public DateTime LockedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ReleasedAtUtc { get; private set; }

    public DateTime? ConfirmedAtUtc { get; private set; }

    public List<InventoryLockItem> Items { get; private set; } = new();

    /// <summary>
    /// 创建库存锁。
    /// </summary>
    public static InventoryLock Create(Guid orderId, DateTime expiresAtUtc, IEnumerable<InventoryLockItem> items)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("订单编号不能为空。", nameof(orderId));
        }

        var lockItems = items.ToList();
        var inventoryLock = new InventoryLock
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ExpiresAtUtc = expiresAtUtc,
            Items = lockItems
        };

        foreach (var item in lockItems)
        {
            item.AttachToLock(inventoryLock.Id);
        }

        return inventoryLock;
    }

    /// <summary>
    /// 释放库存锁。
    /// </summary>
    public void Release()
    {
        if (Status != InventoryLockStatusEnum.Locked)
        {
            return;
        }

        Status = InventoryLockStatusEnum.Released;
        ReleasedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// 确认库存锁完成扣减。
    /// </summary>
    public void Confirm()
    {
        if (Status != InventoryLockStatusEnum.Locked)
        {
            return;
        }

        Status = InventoryLockStatusEnum.Confirmed;
        ConfirmedAtUtc = DateTime.UtcNow;
    }
}
