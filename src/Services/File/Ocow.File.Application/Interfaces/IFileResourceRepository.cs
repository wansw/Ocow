using Ocow.Files.Domain.Models;

namespace Ocow.Files.Application.Interfaces;

/// <summary>
/// 文件资源仓储接口，用于持久化上传文件元数据。
/// </summary>
public interface IFileResourceRepository
{
    /// <summary>
    /// 新增文件资源元数据。
    /// </summary>
    Task AddAsync(FileResource resource, CancellationToken cancellationToken = default);
}
