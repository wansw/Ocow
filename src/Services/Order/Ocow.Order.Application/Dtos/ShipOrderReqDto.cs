namespace Ocow.Order.Application.Dtos;

/// <summary>
/// 后台订单发货请求 DTO。
/// </summary>
public class ShipOrderReqDto
{
    public string ExpressCompany { get; init; } = string.Empty;

    public string ExpressNo { get; init; } = string.Empty;
}
