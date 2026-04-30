using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocow.InternalAuth.Interfaces;

namespace Ocow.InternalAuth.Filters;

/// <summary>
/// HMAC 签名校验过滤器，用于保护高风险内部接口。
/// </summary>
public class RequireHmacSignatureAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// 校验当前请求是否携带合法 HMAC 签名。
    /// </summary>
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        var signatureService = context.HttpContext.RequestServices.GetService(typeof(IHmacSignatureService)) as IHmacSignatureService;
        if (signatureService is null)
        {
            context.Result = new UnauthorizedObjectResult("HMAC 签名服务未注册。");
            return Task.CompletedTask;
        }

        var serviceName = request.Headers["X-Service-Name"].ToString();
        var timestamp = request.Headers["X-Timestamp"].ToString();
        var nonce = request.Headers["X-Nonce"].ToString();
        var bodyHash = request.Headers["X-Body-SHA256"].ToString();
        var signature = request.Headers["X-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(serviceName) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(signature))
        {
            context.Result = new UnauthorizedObjectResult("内部接口缺少 HMAC 签名头。");
            return Task.CompletedTask;
        }

        var valid = signatureService.Validate(serviceName, request.Method, request.Path, timestamp, nonce, bodyHash, signature);
        if (!valid)
        {
            context.Result = new UnauthorizedObjectResult("HMAC 签名校验失败。");
        }

        return Task.CompletedTask;
    }
}
