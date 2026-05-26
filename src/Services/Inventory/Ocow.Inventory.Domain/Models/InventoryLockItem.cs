using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Inventory.Domain.Models;

/// <summary>
/// 库存锁明细实体，用于记录订单中每个 SKU 的锁定数量。
/// </summary>
[Table("inventory_lock_items")]
public class InventoryLockItem
{
    [Key]
    public Guid Id { get; private set; }

    public Guid InventoryLockId { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid SkuId { get; private set; }

    public int Quantity { get; private set; }

    public InventoryLock? InventoryLock { get; private set; }

    /// <summary>
    /// 创建库存锁明细。
    /// </summary>
    public static InventoryLockItem Create(Guid productId, Guid skuId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "锁定数量必须大于 0。");
        }

        return new InventoryLockItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SkuId = skuId,
            Quantity = quantity
        };
    }

    /// <summary>
    /// 绑定库存锁编号。
    /// </summary>
    internal void AttachToLock(Guid inventoryLockId)
    {
        InventoryLockId = inventoryLockId;
    }
}
