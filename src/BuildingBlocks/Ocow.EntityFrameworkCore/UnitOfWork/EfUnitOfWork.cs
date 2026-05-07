using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Abstractions;

namespace Ocow.EntityFrameworkCore.UnitOfWork;

/// <summary>
/// EF Core 工作单元实现，用于统一提交变更和封装数据库事务。
/// </summary>
public class EfUnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    /// <summary>
    /// 创建 EF Core 工作单元。
    /// </summary>
    public EfUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 保存当前 DbContext 中的变更。
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 在数据库事务中执行指定操作。
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync<object?>(async token =>
        {
            await action(token);
            return null;
        }, cancellationToken);
    }

    /// <summary>
    /// 在数据库事务中执行指定操作并返回结果。
    /// </summary>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await action(cancellationToken);
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }
}
