using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Api.Controllers.Client;

/// <summary>
/// 小程序订单接口，用于会员下单、查询和取消订单。
/// </summary>

[Route("api/orders")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.CustomerOnlyPolicy)]
public class OrdersController : BaseController
{
    private readonly IOrderAppService _orderAppService;

    /// <summary>
    /// 创建小程序订单 Controller。
    /// </summary>
    public OrdersController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 创建会员订单。
    /// </summary>
    [HttpPost]
    public async Task<ApiResDto<OrderResDto>> CreateAsync([FromBody] CreateOrderReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.CreateAsync(reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 查询当前会员订单列表。
    /// </summary>
    [HttpGet]
    public async Task<ApiResDto<PageResDto<OrderResDto>>> GetListAsync([FromQuery] Guid customerId, [FromQuery] PageReqDto reqDto, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.GetCustomerOrdersAsync(customerId, reqDto, cancellationToken);
        return Success(result);
    }

    /// <summary>
    /// 查询订单详情。
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
    /// 取消会员订单。
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ApiResDto<OrderResDto>> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _orderAppService.CancelAsync(id, cancellationToken);
        return Success(result);
    }
}
