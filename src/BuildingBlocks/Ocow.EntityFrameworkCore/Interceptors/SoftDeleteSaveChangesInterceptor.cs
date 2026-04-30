using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ocow.EntityFrameworkCore.Abstractions;

namespace Ocow.EntityFrameworkCore.Interceptors;

/// <summary>
/// 软删除保存拦截器，用于将删除操作转换为软删除标记。
/// </summary>
public class SoftDeleteSaveChangesInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// 保存前转换软删除实体状态。
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// 异步保存前转换软删除实体状态。
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 将删除实体转换为软删除标记。
    /// </summary>
    private static void ApplySoftDelete(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        foreach (var entry in dbContext.ChangeTracker.Entries<ISoftDelete>().Where(x => x.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTime.UtcNow;
        }
    }
}
