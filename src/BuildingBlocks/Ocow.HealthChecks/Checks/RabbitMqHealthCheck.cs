using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocow.HealthChecks.Options;
using RabbitMQ.Client;

namespace Ocow.HealthChecks.Checks;

/// <summary>
/// RabbitMQ 健康检查，用于验证 Broker 连接和账号权限是否可用。
/// </summary>
internal class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqHealthCheckOption _option;

    /// <summary>
    /// 创建 RabbitMQ 健康检查。
    /// </summary>
    public RabbitMqHealthCheck(RabbitMqHealthCheckOption option)
    {
        _option = option;
    }

    /// <summary>
    /// 执行 RabbitMQ 连接检查。
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_option.HostName))
        {
            return HealthCheckResult.Unhealthy("RabbitMQ 主机未配置。");
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _option.HostName,
                Port = _option.Port,
                UserName = _option.UserName,
                Password = _option.Password,
                VirtualHost = string.IsNullOrWhiteSpace(_option.VirtualHost) ? "/" : _option.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
            };

            await using var connection = await factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(null, cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ 可用。");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ 不可用。", exception);
        }
    }
}
