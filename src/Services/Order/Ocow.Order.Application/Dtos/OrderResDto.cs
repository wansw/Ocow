using Ocow.Order.Domain.Enums;

namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 订单响应 DTO。
/// </summary>
public class OrderResDto
{
    /// <summary>
    /// 订单编号。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 下单会员编号。
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// 订单业务单号。
    /// </summary>
    public string OrderNo { get; init; } = string.Empty;

    /// <summary>
    /// 订单状态。
    /// </summary>
    public OrderStatusEnum Status { get; init; }

    /// <summary>
    /// 订单总金额。
    /// </summary>
    public decimal TotalAmount { get; init; }

    /// <summary>
    /// 订单创建时间。
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
