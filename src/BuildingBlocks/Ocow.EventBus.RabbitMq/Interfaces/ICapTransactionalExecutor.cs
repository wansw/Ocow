using Microsoft.EntityFrameworkCore;
using Ocow.EventBus.Abstractions.Interfaces;

namespace Ocow.EventBus.RabbitMq.Interfaces;

/// <summary>
/// CAP 事务执行器抽象，用于保证业务数据写入和集成事件发布进入同一数据库事务。
/// </summary>
public interface ICapTransactionalExecutor<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// 在 CAP 事务中执行业务操作并保存数据库变更。
    /// </summary>
    Task ExecuteAsync(
        Func<TDbContext, IEventBus, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 在 CAP 事务中执行业务操作、保存数据库变更并返回结果。
    /// </summary>
    Task<TResult> ExecuteAsync<TResult>(
        Func<TDbContext, IEventBus, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
