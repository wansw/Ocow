namespace Ocow.ERP.Dtos;

/// <summary>
/// 外部 ERP 订单明细响应 DTO，用于表达 ERP 返回的标准化商品行。
/// </summary>
public class ExternalErpOrderItemResDto
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
    /// 商品单价。
    /// </summary>
    public decimal UnitPrice { get; init; }
}
