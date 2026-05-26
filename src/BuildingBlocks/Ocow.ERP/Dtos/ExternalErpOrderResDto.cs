namespace Ocow.ERP.Dtos;

/// <summary>
/// 外部 ERP 订单响应 DTO，用于表达 ERP 返回的标准化订单。
/// </summary>
public class ExternalErpOrderResDto
{
    /// <summary>
    /// 外部 ERP 订单编号，用于和来源系统一起做幂等。
    /// </summary>
    public string ExternalOrderId { get; init; } = string.Empty;

    /// <summary>
    /// 会员编号。
    /// </summary>
    public Guid CustomerId { get; init; }

    /// <summary>
    /// 外部订单创建时间。
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 外部订单明细集合。
    /// </summary>
    public IReadOnlyList<ExternalErpOrderItemResDto> Items { get; init; } = Array.Empty<ExternalErpOrderItemResDto>();
}
