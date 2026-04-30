namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 订单明细请求 DTO。
/// </summary>
public class OrderItemReqDto
{
    /// <summary>
    /// 商品编号。
    /// </summary>
    public Guid ProductId { get; init; }

    /// <summary>
    /// SKU 编号。
    /// </summary>
    public Guid SkuId { get; init; }

    /// <summary>
    /// 商品名称快照。
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// 购买数量。
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 商品成交单价。
    /// </summary>
    public decimal UnitPrice { get; init; }
}
