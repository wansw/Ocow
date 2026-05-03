using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocow.Order.Domain.Models;

/// <summary>
/// 订单明细领域模型，用于记录商品快照和购买数量。
/// </summary>
[Table("order_items")]
public class OrderItemModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid SkuId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// 所属订单，用于表达订单明细到订单的外键关系。
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    [InverseProperty(nameof(OrderModel.Items))]
    public OrderModel? Order { get; set; }
}
