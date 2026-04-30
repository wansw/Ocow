using Ocow.Order.Application.Dtos;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Application.Interfaces;

/// <summary>
/// 订单应用服务接口，用于编排下单、取消、发货和同步用例。
/// </summary>
public interface IOrderAppService
{
    /// <summary>
    /// 创建会员订单。
    /// </summary>
    Task<OrderResDto> CreateAsync(CreateOrderReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询会员订单列表。
    /// </summary>
    Task<PageResDto<OrderResDto>> GetCustomerOrdersAsync(Guid customerId, PageReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询订单详情。
    /// </summary>
    Task<OrderResDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取消订单。
    /// </summary>
    Task<OrderResDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询后台订单列表。
    /// </summary>
    Task<PageResDto<OrderResDto>> GetAdminOrdersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 后台执行订单发货。
    /// </summary>
    Task<OrderResDto> ShipAsync(Guid id, ShipOrderReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 同步 ERP 订单数据。
    /// </summary>
    Task<int> SyncErpOrdersAsync(SyncErpOrdersReqDto reqDto, CancellationToken cancellationToken = default);
}
