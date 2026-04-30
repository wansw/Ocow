namespace Ocow.Order.Domain.Models;

/// <summary>
/// 订单明细领域模型，用于记录商品快照和购买数量。
/// </summary>
public class OrderItemModel
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid SkuId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
