using Microsoft.EntityFrameworkCore;
using Ocow.Files.Domain.Enums;
using Ocow.Files.Domain.Models;

namespace Ocow.Files.Infrastructure.Data;

/// <summary>
/// 文件服务数据库上下文，用于映射文件资源元数据表。
/// </summary>
public class FileDbContext : DbContext
{
    /// <summary>
    /// 创建文件服务数据库上下文。
    /// </summary>
    public FileDbContext(DbContextOptions<FileDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 文件资源表。
    /// </summary>
    public DbSet<FileResource> FileResources => Set<FileResource>();

    /// <summary>
    /// 配置文件资源表字段转换和索引。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileResource>(entity =>
        {
            entity.Property(x => x.FileCategory)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(x => x.StorageType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(FileStorageTypeEnum.Local);
            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(FileResourceStatusEnum.Normal);

            entity.HasIndex(x => x.ObjectKey);
            entity.HasIndex(x => new { x.BizType, x.BizId });
            entity.HasIndex(x => x.CreatedAt);
        });
    }
}
