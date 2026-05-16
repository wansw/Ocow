using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.AspNetCore.Controllers;
using Ocow.Shared.Dtos; 

namespace Ocow.Order.Api.Controllers.Admin;

/// <summary>
/// 后台订单接口，用于管理端查询订单和执行发货。
/// </summary>

[Route("api/admin/orders")]
[Tags("后台订单")]
public class AdminOrdersController : AdminController
{
    private readonly IOrderAppService _orderAppService;

    /// <summary>
    /// 创建后台订单 Controller。
    /// </summary>
    public AdminOrdersController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 查询后台订单列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResDto<PageResDto<OrderResDto>>> GetListAsync([FromQuery] PageReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.GetAdminOrdersAsync(reqDto, cancellationToken);
        return Success(result);
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
            return NotFoundRes<OrderResDto>("ORDER_NOT_FOUND", "订单不存在。");
        }

        return Success(result);
    }

    /// <summary>
    /// 后台执行订单发货。
    /// </summary>
    [PermissionAuthorize("order.ship")]
    [HttpPut("{id:guid}/ship")]
    public async Task<ApiResDto<OrderResDto>> ShipAsync(Guid id, [FromBody] ShipOrderReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.ShipAsync(id, reqDto, cancellationToken);
        return Success(result);
    }
}
