namespace Ocow.Order.Application.Dtos;

/// <summary>
/// ERP 订单同步请求 DTO。/// </summary>
public class SyncErpOrdersReqDto
{
    /// <summary>
    /// 同步开始时间。    /// </summary>
    public DateTime? BeginTime { get; init; }

    /// <summary>
    /// 同步结束时间。    /// </summary>
    public DateTime? EndTime { get; init; }
}
