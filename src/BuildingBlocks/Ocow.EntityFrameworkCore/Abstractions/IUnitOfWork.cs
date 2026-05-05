namespace Ocow.EntityFrameworkCore.Abstractions;

/// <summary>
/// 工作单元接口，用于统一提交数据库变更。/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 保存当前上下文中的变更。    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
