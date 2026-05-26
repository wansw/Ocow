using Ocow.ERP.Dtos;
using Ocow.ERP.Interfaces;
using Ocow.ERP.Options;

namespace Ocow.ERP.Services;

/// <summary>
/// Demo ERP 订单客户端，用于本地联调订单同步链路。
/// </summary>
public class DemoErpOrderClient : IErpOrderClient
{
    private static readonly Guid CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SkuId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    /// <summary>
    /// 返回固定格式的 Demo ERP 订单。
    /// </summary>
    public Task<IReadOnlyList<ExternalErpOrderResDto>> GetOrdersAsync(
        ErpConnectionOption option,
        DateTimeOffset fromTime,
        DateTimeOffset toTime,
        CancellationToken cancellationToken = default)
    {
        var externalOrderId = $"{option.ErpCode.ToUpperInvariant()}-{fromTime:yyyyMMddHHmm}-{toTime:yyyyMMddHHmm}";
        IReadOnlyList<ExternalErpOrderResDto> orders = new[]
        {
            new ExternalErpOrderResDto
            {
                ExternalOrderId = externalOrderId,
                CustomerId = CustomerId,
                CreatedAt = fromTime,
                Items =
                [
                    new ExternalErpOrderItemResDto
                    {
                        ProductId = ProductId,
                        SkuId = SkuId,
                        ProductName = "Demo ERP 商品",
                        Quantity = 1,
                        UnitPrice = 10
                    }
                ]
            }
        };

        return Task.FromResult(orders);
    }
}
