namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 后台订单发货请求 DTO。
/// </summary>
public class ShipOrderReqDto
{
    /// <summary>
    /// 物流公司名称。
    /// </summary>
    public string ExpressCompany { get; init; } = string.Empty;

    /// <summary>
    /// 物流单号。
    /// </summary>
    public string ExpressNo { get; init; } = string.Empty;
}
