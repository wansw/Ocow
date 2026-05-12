using Microsoft.EntityFrameworkCore;

namespace Ocow.EntityFrameworkCore.Repositories;

/// <summary>
/// EF Core 仓储基类，用于提供常用实体读写能力。
/// </summary>
public abstract class EfRepositoryBase<TDbContext, TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class
{
    protected EfRepositoryBase(TDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    /// <summary>
    /// 当前仓储使用的数据库上下文。
    /// </summary>
    protected TDbContext DbContext { get; }

    /// <summary>
    /// 当前仓储对应的实体集合。
    /// </summary>
    protected DbSet<TEntity> DbSet { get; }

    /// <summary>
    /// 新增实体。
    /// </summary>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 根据主键查询实体。
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    /// <summary>
    /// 标记实体为已修改。
    /// </summary>
    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    /// <summary>
    /// 删除实体。
    /// </summary>
    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}
