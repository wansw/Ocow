using Ocow.EntityFrameworkCore.Abstractions;
using Ocow.ERP.Interfaces;
using Ocow.ERP.Options;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Order.Domain.Models;
using Ocow.Shared.Dtos;
using OrderEntity = Ocow.Order.Domain.Models.Order;

namespace Ocow.Order.Application.Services;

/// <summary>
/// 订单应用服务，用于编排订单相关用例。
/// </summary>
public class OrderAppService : IOrderAppService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderCreationTransaction _orderCreationTransaction;
    private readonly IOrderCancellationTransaction _orderCancellationTransaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IErpClientFactory _erpClientFactory;

    /// <summary>
    /// 创建订单应用服务。
    /// </summary>
    public OrderAppService(
        IOrderRepository orderRepository,
        IOrderCreationTransaction orderCreationTransaction,
        IOrderCancellationTransaction orderCancellationTransaction,
        IUnitOfWork unitOfWork,
        IErpClientFactory erpClientFactory)
    {
        _orderRepository = orderRepository;
        _orderCreationTransaction = orderCreationTransaction;
        _orderCancellationTransaction = orderCancellationTransaction;
        _unitOfWork = unitOfWork;
        _erpClientFactory = erpClientFactory;
    }

    /// <summary>
    /// 创建会员订单。
    /// </summary>
    public async Task<OrderResDto> CreateAsync(CreateOrderReqDto reqDto, CancellationToken cancellationToken = default)
    {
        if (reqDto.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("会员编号不能为空。", nameof(reqDto));
        }

        if (reqDto.Items.Count == 0)
        {
            throw new ArgumentException("订单商品不能为空。", nameof(reqDto));
        }

        var items = reqDto.Items.Select(item => new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = item.ProductId,
            SkuId = item.SkuId,
            ProductName = item.ProductName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        });

        var order = OrderEntity.Create(reqDto.CustomerId, items);
        foreach (var item in order.Items)
        {
            item.OrderId = order.Id;
        }

        await _orderCreationTransaction.CreateAsync(order, cancellationToken);

        return MapToResDto(order);
    }

    /// <summary>
    /// 查询会员订单列表。
    /// </summary>
    public async Task<PageResDto<OrderResDto>> GetCustomerOrdersAsync(Guid customerId, PageReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var pageIndex = reqDto.GetSafePageIndex();
        var pageSize = reqDto.GetSafePageSize();
        var result = await _orderRepository.GetCustomerOrdersAsync(customerId, pageIndex, pageSize, cancellationToken);

        return new PageResDto<OrderResDto>
        {
            Items = result.Items.Select(MapToResDto).ToList(),
            Total = result.Total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 查询订单详情。
    /// </summary>
    public async Task<OrderResDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : MapToResDto(order);
    }

    /// <summary>
    /// 取消订单。
    /// </summary>
    public async Task<OrderResDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderCancellationTransaction.CancelAsync(id, "用户主动取消订单。", cancellationToken);

        return MapToResDto(order);
    }

    /// <summary>
    /// 查询后台订单列表。
    /// </summary>
    public async Task<PageResDto<OrderResDto>> GetAdminOrdersAsync(PageReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var pageIndex = reqDto.GetSafePageIndex();
        var pageSize = reqDto.GetSafePageSize();
        var result = await _orderRepository.GetAdminOrdersAsync(pageIndex, pageSize, cancellationToken);

        return new PageResDto<OrderResDto>
        {
            Items = result.Items.Select(MapToResDto).ToList(),
            Total = result.Total,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 后台执行订单发货。
    /// </summary>
    public async Task<OrderResDto> ShipAsync(Guid id, ShipOrderReqDto reqDto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken) ??
                    throw new InvalidOperationException("订单不存在。");

        order.Ship(reqDto.ExpressCompany, reqDto.ExpressNo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToResDto(order);
    }

    /// <summary>
    /// 同步 ERP 订单数据。
    /// </summary>
    public async Task<SyncErpOrdersResDto> SyncErpOrdersAsync(SyncErpOrdersReqDto reqDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reqDto.ErpCode))
        {
            throw new ArgumentException("ERP 编码不能为空。", nameof(reqDto));
        }

        var toTime = ResolveToTime(reqDto);
        var fromTime = ResolveFromTime(reqDto, toTime);
        var client = _erpClientFactory.Create(reqDto.ErpCode);
        var externalOrders = await client.GetOrdersAsync(
            new ErpConnectionOption
            {
                SyncConfigId = reqDto.SyncConfigId,
                ErpCode = reqDto.ErpCode
            },
            fromTime,
            toTime,
            cancellationToken);

        var syncedCount = 0;
        var skippedCount = 0;
        foreach (var externalOrder in externalOrders)
        {
            var exists = await _orderRepository.GetByExternalOrderAsync(reqDto.ErpCode, externalOrder.ExternalOrderId, cancellationToken);
            if (exists is not null)
            {
                skippedCount++;
                continue;
            }

            var order = OrderEntity.CreateFromExternal(
                reqDto.ErpCode,
                externalOrder.ExternalOrderId,
                externalOrder.CustomerId,
                externalOrder.Items.Select(item => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    SkuId = item.SkuId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }),
                externalOrder.CreatedAt.UtcDateTime);

            await _orderRepository.AddAsync(order, cancellationToken);
            syncedCount++;
        }

        if (syncedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new SyncErpOrdersResDto
        {
            SyncedCount = syncedCount,
            SkippedCount = skippedCount,
            FromTime = fromTime,
            ToTime = toTime
        };
    }

    /// <summary>
    /// 解析 ERP 同步开始时间。
    /// </summary>
    private static DateTimeOffset ResolveFromTime(SyncErpOrdersReqDto reqDto, DateTimeOffset toTime)
    {
        if (reqDto.FromTime.HasValue)
        {
            return reqDto.FromTime.Value;
        }

        if (reqDto.BeginTime.HasValue)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(reqDto.BeginTime.Value, DateTimeKind.Utc));
        }

        return toTime.AddMinutes(-5);
    }

    /// <summary>
    /// 解析 ERP 同步结束时间。
    /// </summary>
    private static DateTimeOffset ResolveToTime(SyncErpOrdersReqDto reqDto)
    {
        if (reqDto.ToTime.HasValue)
        {
            return reqDto.ToTime.Value;
        }

        if (reqDto.EndTime.HasValue)
        {
            return new DateTimeOffset(DateTime.SpecifyKind(reqDto.EndTime.Value, DateTimeKind.Utc));
        }

        return DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 将领域实体转换为响应 DTO。
    /// </summary>
    private static OrderResDto MapToResDto(OrderEntity order)
    {
        return new OrderResDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNo = order.OrderNo,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };
    }
}
