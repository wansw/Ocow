extern alias CapMySql;
extern alias CapPostgreSql;
extern alias CapSqlServer;

using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocow.EventBus.Abstractions.Interfaces;
using Ocow.EventBus.RabbitMq.Interfaces;
using Ocow.EventBus.RabbitMq.Options;

namespace Ocow.EventBus.RabbitMq.Services;

/// <summary>
/// CAP 事务执行器，用于将 EF Core 数据变更和 CAP Outbox 消息写入同一事务。
/// </summary>
public sealed class CapTransactionalExecutor<TDbContext> : ICapTransactionalExecutor<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ICapPublisher _capPublisher;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CapTransactionalExecutor<TDbContext>> _logger;
    private readonly CapRabbitMqEventBusOptions _options;

    /// <summary>
    /// 创建 CAP 事务执行器。
    /// </summary>
    public CapTransactionalExecutor(
        TDbContext dbContext,
        ICapPublisher capPublisher,
        IEventBus eventBus,
        ILogger<CapTransactionalExecutor<TDbContext>> logger,
        IOptions<CapRabbitMqEventBusOptions> options)
    {
        _dbContext = dbContext;
        _capPublisher = capPublisher;
        _eventBus = eventBus;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 在 CAP 事务中执行业务操作并保存数据库变更。
    /// </summary>
    public Task ExecuteAsync(
        Func<TDbContext, IEventBus, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync<object?>(
            async (dbContext, eventBus, ct) =>
            {
                await operation(dbContext, eventBus, ct);
                return null;
            },
            cancellationToken);
    }

    /// <summary>
    /// 在 CAP 事务中执行业务操作、保存数据库变更并返回结果。
    /// </summary>
    public async Task<TResult> ExecuteAsync<TResult>(
        Func<TDbContext, IEventBus, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await BeginTransactionAsync(
                _dbContext.Database,
                _capPublisher,
                _options.Storage.Provider,
                cancellationToken);

            try
            {
                var result = await operation(_dbContext, _eventBus, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "CAP 事务执行失败，正在回滚数据库事务。");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// 按 CAP 存储 Provider 显式调用对应包的事务扩展，避免多 Provider 同名扩展方法二义性。
    /// </summary>
    private static Task<IDbContextTransaction> BeginTransactionAsync(
        DatabaseFacade database,
        ICapPublisher capPublisher,
        string provider,
        CancellationToken cancellationToken)
    {
        return provider switch
        {
            "SqlServer" => CapSqlServer::DotNetCore.CAP.CapTransactionExtensions.BeginTransactionAsync(
                database,
                capPublisher,
                autoCommit: false,
                cancellationToken),
            "PostgreSql" or "PostgreSQL" or "Postgres" => CapPostgreSql::DotNetCore.CAP.CapTransactionExtensions.BeginTransactionAsync(
                database,
                capPublisher,
                autoCommit: false,
                cancellationToken),
            "MySql" or "MySQL" => CapMySql::DotNetCore.CAP.CapTransactionExtensions.BeginTransactionAsync(
                database,
                capPublisher,
                autoCommit: false,
                cancellationToken),
            _ => throw new NotSupportedException($"不支持的 CAP 存储 Provider：{provider}")
        };
    }
}
