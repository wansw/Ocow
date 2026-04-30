using Ocow.Order.Domain.Enums;

namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 订单响应 DTO。
/// </summary>
public class OrderResDto
{
    public Guid Id { get; init; }

    public Guid CustomerId { get; init; }

    public string OrderNo { get; init; } = string.Empty;

    public OrderStatusEnum Status { get; init; }

    public decimal TotalAmount { get; init; }

    public DateTime CreatedAt { get; init; }
}
