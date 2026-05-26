using Microsoft.EntityFrameworkCore;
using Ocow.Jobs.Api.Models;

namespace Ocow.Jobs.Api.Data;

/// <summary>
/// Jobs 服务数据库上下文，用于映射任务配置表和执行日志表。
/// </summary>
public class JobsDbContext : DbContext
{
    /// <summary>
    /// 创建 Jobs 服务数据库上下文。
    /// </summary>
    public JobsDbContext(DbContextOptions<JobsDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 任务配置表。
    /// </summary>
    public DbSet<JobDefinition> JobDefinitions => Set<JobDefinition>();

    /// <summary>
    /// 任务执行日志表。
    /// </summary>
    public DbSet<JobExecutionLog> JobExecutionLogs => Set<JobExecutionLog>();

    /// <summary>
    /// 配置任务配置和执行日志表的索引关系。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobDefinition>(entity =>
        {
            entity.HasIndex(x => x.Id)
                .IsUnique();
        });

        modelBuilder.Entity<JobExecutionLog>(entity =>
        {
            entity.HasIndex(x => x.JobCode);
            entity.HasOne<JobDefinition>()
                .WithMany()
                .HasForeignKey(x => x.JobDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
