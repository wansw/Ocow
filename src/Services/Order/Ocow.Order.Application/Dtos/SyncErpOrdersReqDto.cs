namespace Ocow.Order.Application.Dtos;

/// <summary>
/// ERP 订单同步请求 DTO
/// </summary>
public class SyncErpOrdersReqDto
{
    /// <summary>
    /// 订单同步配置编号。
    /// </summary>
    public string? SyncConfigId { get; init; }

    /// <summary>
    /// ERP 编码。
    /// </summary>
    public string ErpCode { get; init; } = "demo";

    /// <summary>
    /// 同步开始时间。
    /// </summary>
    public DateTimeOffset? FromTime { get; init; }

    /// <summary>
    /// 同步结束时间。
    /// </summary>
    public DateTimeOffset? ToTime { get; init; }

    /// <summary>
    /// 同步开始时间。    /// </summary>
    public DateTime? BeginTime { get; init; }

    /// <summary>
    /// 同步结束时间。    /// </summary>
    public DateTime? EndTime { get; init; }
}
