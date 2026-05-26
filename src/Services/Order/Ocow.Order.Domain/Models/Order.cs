using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocow.Order.Domain.Enums;

namespace Ocow.Order.Domain.Models;

/// <summary>
/// 订单领域模型，用于承载订单主信息和状态流转。/// </summary>
[Table("orders")]
public class Order
{
    [Key]
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    [Required]
    [MaxLength(64)]
    public string OrderNo { get; set; } = string.Empty;

    public OrderStatusEnum Status { get; private set; } = OrderStatusEnum.PendingPay;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; private set; }

    [MaxLength(64)]
    public string? SourceSystem { get; private set; }

    [MaxLength(128)]
    public string? ExternalOrderId { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CanceledAt { get; private set; }

    public DateTime? ShippedAt { get; private set; }

    [MaxLength(64)]
    public string? ExpressCompany { get; private set; }

    [MaxLength(64)]
    public string? ExpressNo { get; private set; }

    /// <summary>
    /// 订单明细集合，用于表达订单与订单明细的一对多关系。    /// </summary>
    [InverseProperty(nameof(OrderItem.Order))]
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// 创建待支付订单，并计算订单总金额。    /// </summary>
    public static Order Create(Guid customerId, IEnumerable<OrderItem> items)
    {
        var orderItems = items.ToList();
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderNo = $"OC{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            Items = orderItems,
            TotalAmount = orderItems.Sum(x => x.UnitPrice * x.Quantity)
        };
    }

    /// <summary>
    /// 从外部 ERP 订单创建内部订单，并记录外部幂等键。
    /// </summary>
    public static Order CreateFromExternal(
        string sourceSystem,
        string externalOrderId,
        Guid customerId,
        IEnumerable<OrderItem> items,
        DateTime createdAt)
    {
        var order = Create(customerId, items);
        order.SourceSystem = sourceSystem;
        order.ExternalOrderId = externalOrderId;
        order.CreatedAt = createdAt;

        foreach (var item in order.Items)
        {
            item.OrderId = order.Id;
        }

        return order;
    }

    /// <summary>
    /// 取消待支付订单。    /// </summary>
    public void Cancel()
    {
        if (Status != OrderStatusEnum.PendingPay)
        {
            throw new InvalidOperationException("只有待支付订单可以取消。");
        }

        Status = OrderStatusEnum.Canceled;
        CanceledAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 标记订单已发货。    /// </summary>
    public void Ship(string expressCompany, string expressNo)
    {
        if (Status is OrderStatusEnum.Canceled or OrderStatusEnum.Completed)
        {
            throw new InvalidOperationException("当前订单状态不允许发货。");
        }

        Status = OrderStatusEnum.Shipped;
        ExpressCompany = expressCompany;
        ExpressNo = expressNo;
        ShippedAt = DateTime.UtcNow;
    }
}
