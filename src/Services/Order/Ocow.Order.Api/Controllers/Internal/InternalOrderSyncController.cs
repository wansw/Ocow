using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ocow.AspNetCore.Controllers;
using Ocow.AspNetCore.SwaggerApi;
using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Dtos;
using Ocow.Order.Application.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Order.Api.Controllers.Internal;

/// <summary>
/// 内部订单同步接口，用。Scheduler、ERP 等内部服务调用
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Internal)]
[Route("internal/orders/sync")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.InternalOnlyPolicy)]
[Tags("内部订单同步")]
public class InternalOrderSyncController : BaseController
{
    private readonly IOrderAppService _orderAppService;

    /// <summary>
    /// 创建内部订单同步 Controller。    
    /// </summary>
    public InternalOrderSyncController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// 同步 ERP 订单数据。     [FromBody] SyncErpOrdersReqDto reqDto,
    /// </summary>
    [HttpPost("erp")]
    public async Task<ApiResDto<SyncErpOrdersResDto>> SyncErpAsync(CancellationToken cancellationToken)
    {
        SyncErpOrdersReqDto reqDto = new();
        var result = await _orderAppService.SyncErpOrdersAsync(reqDto, cancellationToken);
        return Success(result);
    }
}
