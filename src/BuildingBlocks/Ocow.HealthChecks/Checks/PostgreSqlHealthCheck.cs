using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Ocow.HealthChecks.Checks;

/// <summary>
/// PostgreSQL 健康检查，用于验证数据库连接和基础查询是否可用。
/// </summary>
internal class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    /// <summary>
    /// 创建 PostgreSQL 健康检查。
    /// </summary>
    public PostgreSqlHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// 执行 PostgreSQL 连接和 SELECT 1 检查。
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return HealthCheckResult.Unhealthy("PostgreSQL 连接字符串未配置。");
        }

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("PostgreSQL 可用。");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL 不可用。", exception);
        }
    }
}
