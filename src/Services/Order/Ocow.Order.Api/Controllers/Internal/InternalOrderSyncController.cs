using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Api.Controllers.Internal;

/// <summary>
/// 内部订单同步接口，用。Scheduler、ERP 等内部服务调用。/// </summary>
[ApiController]
[Route("internal/orders/sync")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.InternalOnlyPolicy)]
public class InternalOrderSyncController : ControllerBase
{
    private readonly IOrderAppService _orderAppService;

    /// <summary>
    /// 创建内部订单同步 Controller。    /// </summary>
    public InternalOrderSyncController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 同步 ERP 订单数据。    /// </summary>
    [HttpPost("erp")]
    public async Task<ActionResult<ApiResDto<int>>> SyncErpAsync([FromBody] SyncErpOrdersReqDto reqDto, CancellationToken cancellationToken)
    {
        var count = await _orderAppService.SyncErpOrdersAsync(reqDto, cancellationToken);
        return ApiResDto<int>.Ok(count, HttpContext.TraceIdentifier);
    }
}
