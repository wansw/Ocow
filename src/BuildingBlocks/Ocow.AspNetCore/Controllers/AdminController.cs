using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.AspNetCore.SwaggerApi;
using Ocow.Auth.Extensions;

namespace Ocow.AspNetCore.Controllers;

/// <summary>
/// 后台接口控制器基类，用于统一后台 Swagger 分组和 Admin 授权策略。
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Admin)]
[Authorize(Policy = AuthServiceCollectionExtensions.AdminOnlyPolicy)]
public abstract class AdminController : BaseController
{
}
