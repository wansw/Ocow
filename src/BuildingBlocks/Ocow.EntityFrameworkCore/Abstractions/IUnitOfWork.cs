namespace Ocow.EntityFrameworkCore.Abstractions;

/// <summary>
/// 工作单元接口，用于统一提交数据库变更和执行事务。
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 保存当前上下文中的变更。
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 在数据库事务中执行指定操作。
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// 在数据库事务中执行指定操作并返回结果。
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
}
