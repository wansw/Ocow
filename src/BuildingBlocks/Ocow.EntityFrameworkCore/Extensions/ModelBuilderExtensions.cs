using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ocow.EntityFrameworkCore.Abstractions;

namespace Ocow.EntityFrameworkCore.Extensions;

/// <summary>
/// ModelBuilder 扩展，用于注册通用模型约定。/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// 为实现软删除接口的实体添加全局查询过滤器。    /// </summary>
    public static ModelBuilder ApplyOcowSoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        return modelBuilder;
    }
}
