using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocow.Gateway.Options;
using Ocow.Gateway.Services;
using Ocow.Redis.Interfaces;

namespace Ocow.Gateway.Middleware;

/// <summary>
/// 网关安全中间件，用于在 Ocelot 转发前执行 IP 访问控制、限流、入口认证和粗粒度授权。
/// </summary>
public sealed class GatewaySecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GatewaySecurityOption _option;
    private readonly IRedisRateLimiter _rateLimiter;
    private readonly IGatewayRouteAuthorizer _authorizer;
    private readonly IGatewayForwardedUserTokenService _tokenService;
    private readonly ILogger<GatewaySecurityMiddleware> _logger;

    /// <summary>
    /// 创建网关安全中间件。
    /// </summary>
    public GatewaySecurityMiddleware(
        RequestDelegate next,
        IOptions<GatewaySecurityOption> option,
        IRedisRateLimiter rateLimiter,
        IGatewayRouteAuthorizer authorizer,
        IGatewayForwardedUserTokenService tokenService,
        ILogger<GatewaySecurityMiddleware> logger)
    {
        _next = next;
        _option = option.Value;
        _rateLimiter = rateLimiter;
        _authorizer = authorizer;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// 处理当前请求的网关安全检查。
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_option.Enabled)
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context, _option.TrustForwardedForHeader);
        if (GatewayIpAddressMatcher.MatchesAny(clientIp, _option.IpBlacklist))
        {
            _logger.LogWarning("Gateway rejected blacklisted IP {ClientIp} for {Path}", clientIp, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (_option.IpWhitelist.Count > 0 && !GatewayIpAddressMatcher.MatchesAny(clientIp, _option.IpWhitelist))
        {
            _logger.LogWarning("Gateway rejected non-whitelisted IP {ClientIp} for {Path}", clientIp, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var routePolicy = FindRoutePolicy(context.Request.Path);
        await ApplyRateLimitAsync(context, clientIp, routePolicy);
        if (context.Response.HasStarted || context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            return;
        }

        if (routePolicy is null)
        {
            if (_option.DenyUnmatchedRoutes)
            {
                _logger.LogWarning("Gateway rejected unmatched route {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next(context);
            return;
        }

        if (routePolicy.AllowAnonymous)
        {
            await _next(context);
            return;
        }

        var authorizationResult = await _authorizer.AuthorizeAsync(context, routePolicy, context.RequestAborted);
        if (!authorizationResult.IsAuthenticated)
        {
            _logger.LogInformation("Gateway rejected unauthenticated request for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (!authorizationResult.IsAuthorized)
        {
            _logger.LogInformation("Gateway rejected unauthorized request for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (routePolicy.ForwardUserIdentity)
        {
            var token = _tokenService.CreateToken(context.User);
            context.Request.Headers.Authorization = $"Bearer {token}";
            context.Request.Headers["X-Gateway-Forwarded-User"] = "true";
        }

        await _next(context);
    }

    /// <summary>
    /// 根据客户端 IP 和路由策略执行 Redis 固定窗口限流。
    /// </summary>
    private async Task ApplyRateLimitAsync(HttpContext context, System.Net.IPAddress? clientIp, GatewayRoutePolicyOption? routePolicy)
    {
        if (!_option.RateLimit.Enabled)
        {
            return;
        }

        var window = TimeSpan.FromSeconds(_option.RateLimit.WindowSeconds);
        var routeKey = routePolicy?.PathPrefix ?? "unmatched";
        var clientKey = clientIp?.ToString() ?? "unknown";
        var result = await _rateLimiter.TryAcquireAsync($"gateway:rate-limit:{clientKey}:{routeKey}", _option.RateLimit.PermitLimit, window, context.RequestAborted);

        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        if (result.Allowed)
        {
            return;
        }

        if (result.RetryAfter.HasValue)
        {
            context.Response.Headers.RetryAfter = Math.Ceiling(result.RetryAfter.Value.TotalSeconds).ToString();
        }

        _logger.LogWarning("Gateway rate limit exceeded for {ClientIp} on {Path}", clientIp, context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    }

    /// <summary>
    /// 按最长路径前缀查找当前请求对应的网关路由策略。
    /// </summary>
    private GatewayRoutePolicyOption? FindRoutePolicy(PathString path)
    {
        return _option.Routes
            .Where(route => path.StartsWithSegments(new PathString(route.PathPrefix), StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(route => route.PathPrefix.Length)
            .FirstOrDefault();
    }

    /// <summary>
    /// 获取客户端 IP，优先读取反向代理传入的 X-Forwarded-For。
    /// </summary>
    private static System.Net.IPAddress? GetClientIpAddress(HttpContext context, bool trustForwardedForHeader)
    {
        if (trustForwardedForHeader)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                var firstIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (System.Net.IPAddress.TryParse(firstIp, out var forwardedIp))
                {
                    return forwardedIp;
                }
            }
        }

        return context.Connection.RemoteIpAddress;
    }
}
