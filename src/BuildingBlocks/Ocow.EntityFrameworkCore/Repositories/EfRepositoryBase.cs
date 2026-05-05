using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Abstractions;

namespace Ocow.EntityFrameworkCore.Repositories;

/// <summary>
/// EF Core 仓储基类，用于提供常用实体读写能力。/// </summary>
public abstract class EfRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly DbContext _dbContext;

    protected EfRepositoryBase(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 新增实体。    /// </summary>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// 根据主键查询实体。    /// </summary>
    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken);
    }

    /// <summary>
    /// 删除实体。    /// </summary>
    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    /// <summary>
    /// 保存当前仓储变更。    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
