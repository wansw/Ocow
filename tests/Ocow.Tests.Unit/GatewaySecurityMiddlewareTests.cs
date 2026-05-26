using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ocow.Gateway.Middleware;
using Ocow.Gateway.Options;
using Ocow.Redis.Interfaces;
using Ocow.Redis.Models;

namespace Ocow.Tests.Unit;

/// <summary>
/// 网关安全中间件测试，用于验证入口认证、IP 黑白名单、限流和粗粒度路由权限。
/// </summary>
public class GatewaySecurityMiddlewareTests
{
    /// <summary>
    /// 验证匿名路由不会触发授权检查。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenRouteAllowsAnonymous_ShouldSkipAuthorization()
    {
        var authorizer = new FakeGatewayRouteAuthorizer(GatewayRouteAuthorizationResult.Authorized());
        var middleware = CreateMiddleware(CreateOption(routes:
        [
            new GatewayRoutePolicyOption
            {
                PathPrefix = "/api/auth",
                AllowAnonymous = true
            }
        ]), authorizer: authorizer);
        var context = CreateContext("/api/auth/login", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.True(context.Items.ContainsKey("next"));
        Assert.Equal(0, authorizer.CallCount);
    }

    /// <summary>
    /// 验证客户路由缺少有效 Token 时返回 401。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenClientRouteUnauthenticated_ShouldReturn401()
    {
        var middleware = CreateMiddleware(CreateOption(routes:
        [
            new GatewayRoutePolicyOption
            {
                PathPrefix = "/api/orders",
                AuthenticationScheme = "CustomerJwt",
                AuthorizationPolicy = "CustomerOnly"
            }
        ]), authorizer: new FakeGatewayRouteAuthorizer(GatewayRouteAuthorizationResult.Unauthenticated()));
        var context = CreateContext("/api/orders", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey("next"));
    }

    /// <summary>
    /// 验证后台路由认证通过但粗粒度策略不通过时返回 403。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAdminRouteUnauthorized_ShouldReturn403()
    {
        var middleware = CreateMiddleware(CreateOption(routes:
        [
            new GatewayRoutePolicyOption
            {
                PathPrefix = "/api/admin",
                AuthenticationScheme = "AdminJwt",
                AuthorizationPolicy = "AdminOnly"
            }
        ]), authorizer: new FakeGatewayRouteAuthorizer(GatewayRouteAuthorizationResult.Forbidden()));
        var context = CreateContext("/api/admin/orders", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey("next"));
    }

    /// <summary>
    /// 验证命中 IP 黑名单时直接拒绝请求。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenIpBlacklisted_ShouldReturn403()
    {
        var middleware = CreateMiddleware(CreateOption(ipBlacklist: ["10.0.0.8"]));
        var context = CreateContext("/api/orders", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey("next"));
    }

    /// <summary>
    /// 验证启用白名单后非白名单 IP 会被拒绝。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenWhitelistConfiguredAndIpNotAllowed_ShouldReturn403()
    {
        var middleware = CreateMiddleware(CreateOption(ipWhitelist: ["10.0.0.1"]));
        var context = CreateContext("/health/order", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey("next"));
    }

    /// <summary>
    /// 验证默认不信任客户端伪造的 X-Forwarded-For，避免绕过 IP 白名单。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenForwardedForSpoofedAndNotTrusted_ShouldUseRemoteIp()
    {
        var middleware = CreateMiddleware(CreateOption(
            ipWhitelist: ["10.0.0.1"],
            routes:
            [
                new GatewayRoutePolicyOption
                {
                    PathPrefix = "/health",
                    AllowAnonymous = true
                }
            ]));
        var context = CreateContext("/health/order", "10.0.0.8");
        context.Request.Headers["X-Forwarded-For"] = "10.0.0.1";

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey("next"));
    }

    /// <summary>
    /// 验证 Redis 限流超限时返回 429。
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenRateLimitExceeded_ShouldReturn429()
    {
        var limiter = new FakeRedisRateLimiter(new RedisRateLimitResult
        {
            Allowed = false,
            Limit = 1,
            Used = 2,
            Remaining = 0,
            RetryAfter = TimeSpan.FromSeconds(30)
        });
        var middleware = CreateMiddleware(CreateOption(rateLimitEnabled: true), limiter: limiter);
        var context = CreateContext("/health/order", "10.0.0.8");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.Equal("30", context.Response.Headers.RetryAfter);
        Assert.False(context.Items.ContainsKey("next"));
    }

    private static GatewaySecurityMiddleware CreateMiddleware(
        GatewaySecurityOption option,
        IRedisRateLimiter? limiter = null,
        IGatewayRouteAuthorizer? authorizer = null)
    {
        return new GatewaySecurityMiddleware(
            async context =>
            {
                context.Items["next"] = true;
                context.Response.StatusCode = StatusCodes.Status200OK;
                await Task.CompletedTask;
            },
            Options.Create(option),
            limiter ?? new FakeRedisRateLimiter(new RedisRateLimitResult
            {
                Allowed = true,
                Limit = 100,
                Used = 1,
                Remaining = 99
            }),
            authorizer ?? new FakeGatewayRouteAuthorizer(GatewayRouteAuthorizationResult.Authorized()),
            NullLogger<GatewaySecurityMiddleware>.Instance);
    }

    private static GatewaySecurityOption CreateOption(
        bool rateLimitEnabled = false,
        string[]? ipWhitelist = null,
        string[]? ipBlacklist = null,
        IReadOnlyCollection<GatewayRoutePolicyOption>? routes = null)
    {
        return new GatewaySecurityOption
        {
            Enabled = true,
            IpWhitelist = ipWhitelist ?? [],
            IpBlacklist = ipBlacklist ?? [],
            RateLimit = new GatewayRateLimitOption
            {
                Enabled = rateLimitEnabled,
                PermitLimit = 1,
                WindowSeconds = 60
            },
            Routes = routes ?? []
        };
    }

    private static DefaultHttpContext CreateContext(string path, string remoteIp)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        context.Response.Body = new MemoryStream();
        return context;
    }

    private sealed class FakeGatewayRouteAuthorizer : IGatewayRouteAuthorizer
    {
        private readonly GatewayRouteAuthorizationResult _result;

        public int CallCount { get; private set; }

        /// <summary>
        /// 创建测试用网关路由授权器。
        /// </summary>
        public FakeGatewayRouteAuthorizer(GatewayRouteAuthorizationResult result)
        {
            _result = result;
        }

        /// <summary>
        /// 返回预设的授权结果。
        /// </summary>
        public Task<GatewayRouteAuthorizationResult> AuthorizeAsync(
            HttpContext context,
            GatewayRoutePolicyOption routePolicy,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeRedisRateLimiter : IRedisRateLimiter
    {
        private readonly RedisRateLimitResult _result;

        /// <summary>
        /// 创建测试用 Redis 限流器。
        /// </summary>
        public FakeRedisRateLimiter(RedisRateLimitResult result)
        {
            _result = result;
        }

        /// <summary>
        /// 返回预设的限流结果。
        /// </summary>
        public Task<RedisRateLimitResult> TryAcquireAsync(string key, long limit, TimeSpan window, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }
}
