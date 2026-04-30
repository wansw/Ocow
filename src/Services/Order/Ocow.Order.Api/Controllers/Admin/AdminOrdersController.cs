using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Api.Controllers.Admin;

/// <summary>
/// 后台订单接口，用于管理端查询订单和执行发货。
/// </summary>
[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.AdminOnlyPolicy)]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderAppService _orderAppService;

    public AdminOrdersController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 查询后台订单列表。
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResDto<PageResDto<OrderResDto>>>> GetListAsync([FromQuery] PageReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.GetAdminOrdersAsync(reqDto, cancellationToken);
        return ApiResDto<PageResDto<OrderResDto>>.Ok(result, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 查询后台订单详情。
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResDto<OrderResDto>>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResDto<OrderResDto>.Fail("ORDER_NOT_FOUND", "订单不存在。", HttpContext.TraceIdentifier));
        }

        return ApiResDto<OrderResDto>.Ok(result, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 后台执行订单发货。
    /// </summary>
    [Authorize(Policy = InternalAuthServiceCollectionExtensions.OrderShipPolicy)]
    [HttpPut("{id:guid}/ship")]
    public async Task<ActionResult<ApiResDto<OrderResDto>>> ShipAsync(Guid id, [FromBody] ShipOrderReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.ShipAsync(id, reqDto, cancellationToken);
        return ApiResDto<OrderResDto>.Ok(result, HttpContext.TraceIdentifier);
    }
}
