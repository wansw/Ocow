using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Identity.Application.Dtos;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Client;

/// <summary>
/// 小程序当前身份接口，用于查询当前登录会员身份。/// </summary>
[ApiController]
[Route("api/auth")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.CustomerOnlyPolicy)]
public class ClientProfileController : ControllerBase
{
    /// <summary>
    /// 查询当前小程序用户身份。    /// </summary>
    [HttpGet("me")]
    public ActionResult<ApiResDto<ClientProfileResDto>> GetMe()
    {
        var memberId = User.FindFirst("memberId")?.Value;
        var openId = User.FindFirst("openid")?.Value ?? string.Empty;

        var result = new ClientProfileResDto
        {
            MemberId = Guid.TryParse(memberId, out var id) ? id : Guid.Empty,
            OpenId = openId
        };

        return ApiResDto<ClientProfileResDto>.Ok(result, HttpContext.TraceIdentifier);
    }
}
