namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 订单明细请求 DTO。
/// </summary>
public class OrderItemReqDto
{
    public Guid ProductId { get; init; }

    public Guid SkuId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}
