using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocow.HealthChecks.Options;
using StackExchange.Redis;

namespace Ocow.HealthChecks.Checks;

/// <summary>
/// Redis 健康检查，用于验证 Redis 连接和 Ping 是否可用。
/// </summary>
internal class RedisHealthCheck : IHealthCheck
{
    private readonly RedisHealthCheckOption _option;

    /// <summary>
    /// 创建 Redis 健康检查。
    /// </summary>
    public RedisHealthCheck(RedisHealthCheckOption option)
    {
        _option = option;
    }

    /// <summary>
    /// 执行 Redis 连接和 Ping 检查。
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_option.Configuration))
        {
            return HealthCheckResult.Unhealthy("Redis 连接字符串未配置。");
        }

        try
        {
            var configuration = ConfigurationOptions.Parse(_option.Configuration);
            configuration.AbortOnConnectFail = false;
            if (_option.DefaultDatabase.HasValue)
            {
                configuration.DefaultDatabase = _option.DefaultDatabase;
            }

            using var connection = await ConnectionMultiplexer.ConnectAsync(configuration);
            await connection.GetDatabase(_option.DefaultDatabase ?? -1).PingAsync();

            return HealthCheckResult.Healthy("Redis 可用。");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis 不可用。", exception);
        }
    }
}
