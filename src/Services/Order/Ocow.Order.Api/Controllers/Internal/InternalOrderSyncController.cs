using Microsoft.AspNetCore.Mvc;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Api.Controllers.Internal;

/// <summary>
/// 内部订单同步接口，用于 Scheduler、ERP 等内部服务调用。
/// </summary>
[ApiController]
[Route("internal/orders/sync")]
public class InternalOrderSyncController : ControllerBase
{
    private readonly IOrderAppService _orderAppService;

    public InternalOrderSyncController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 同步 ERP 订单数据。
    /// </summary>
    [HttpPost("erp")]
    public async Task<ActionResult<ApiResDto<int>>> SyncErpAsync([FromBody] SyncErpOrdersReqDto reqDto, CancellationToken cancellationToken)
    {
        var count = await _orderAppService.SyncErpOrdersAsync(reqDto, cancellationToken);
        return ApiResDto<int>.Ok(count, HttpContext.TraceIdentifier);
    }
}
