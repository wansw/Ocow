using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ocow.EntityFrameworkCore.Abstractions;

namespace Ocow.EntityFrameworkCore.Interceptors;

/// <summary>
/// 审计字段保存拦截器，用于自动维护创建时间和更新时间。
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// 保存前自动设置审计时间。
    /// </summary>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// 异步保存前自动设置审计时间。
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 设置当前上下文中实体的审计字段。
    /// </summary>
    private static void SetAuditFields(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = entry.Entity.CreatedAt == default ? now : entry.Entity.CreatedAt;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
