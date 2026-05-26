using Ocow.ERP.Dtos;
using Ocow.ERP.Options;

namespace Ocow.ERP.Interfaces;

/// <summary>
/// ERP 订单客户端接口，用于从外部 ERP 拉取标准化订单数据。
/// </summary>
public interface IErpOrderClient
{
    /// <summary>
    /// 按时间窗口获取外部 ERP 订单。
    /// </summary>
    Task<IReadOnlyList<ExternalErpOrderResDto>> GetOrdersAsync(
        ErpConnectionOption option,
        DateTimeOffset fromTime,
        DateTimeOffset toTime,
        CancellationToken cancellationToken = default);
}
