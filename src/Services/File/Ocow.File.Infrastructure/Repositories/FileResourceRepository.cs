using Ocow.Files.Application.Interfaces;
using Ocow.Files.Domain.Models;
using Ocow.Files.Infrastructure.Data;

namespace Ocow.Files.Infrastructure.Repositories;

/// <summary>
/// 文件资源仓储实现，用于通过 EF Core 保存文件元数据。
/// </summary>
public class FileResourceRepository : IFileResourceRepository
{
    private readonly FileDbContext _dbContext;

    /// <summary>
    /// 创建文件资源仓储。
    /// </summary>
    public FileResourceRepository(FileDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 新增文件资源元数据。
    /// </summary>
    public async Task AddAsync(FileResource resource, CancellationToken cancellationToken = default)
    {
        _dbContext.FileResources.Add(resource);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
