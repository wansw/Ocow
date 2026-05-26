using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Inventory.Domain.Models;

/// <summary>
/// 库存商品实体，用于维护 SKU 可售库存和锁定库存。
/// </summary>
[Table("inventory_items")]
public class InventoryItem
{
    [Key]
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid SkuId { get; private set; }

    public int AvailableQuantity { get; private set; }

    public int LockedQuantity { get; private set; }

    /// <summary>
    /// 创建库存商品。
    /// </summary>
    public static InventoryItem Create(Guid productId, Guid skuId, int availableQuantity)
    {
        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "可售库存不能小于 0。");
        }

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SkuId = skuId,
            AvailableQuantity = availableQuantity
        };
    }

    /// <summary>
    /// 锁定库存，减少可售库存并增加锁定库存。
    /// </summary>
    public void Lock(int quantity)
    {
        ValidatePositiveQuantity(quantity);
        if (AvailableQuantity < quantity)
        {
            throw new InvalidOperationException("可售库存不足。");
        }

        AvailableQuantity -= quantity;
        LockedQuantity += quantity;
    }

    /// <summary>
    /// 释放库存锁定，把锁定库存归还到可售库存。
    /// </summary>
    public void Release(int quantity)
    {
        ValidatePositiveQuantity(quantity);
        if (LockedQuantity < quantity)
        {
            throw new InvalidOperationException("锁定库存不足，无法释放。");
        }

        LockedQuantity -= quantity;
        AvailableQuantity += quantity;
    }

    /// <summary>
    /// 确认库存扣减，减少锁定库存且不回补可售库存。
    /// </summary>
    public void ConfirmDeduction(int quantity)
    {
        ValidatePositiveQuantity(quantity);
        if (LockedQuantity < quantity)
        {
            throw new InvalidOperationException("锁定库存不足，无法确认扣减。");
        }

        LockedQuantity -= quantity;
    }

    /// <summary>
    /// 校验库存数量必须为正数。
    /// </summary>
    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "库存数量必须大于 0。");
        }
    }
}
