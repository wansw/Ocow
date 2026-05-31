using Ocow.Files.Application.Models;

namespace Ocow.Files.Application.Interfaces;

/// <summary>
/// 文件存储提供者接口，用于屏蔽本地存储和对象存储差异。
/// </summary>
public interface IFileStorageProvider
{
    /// <summary>
    /// 保存上传文件并返回存储对象键。
    /// </summary>
    Task<FileStorageSaveResult> SaveAsync(FileStorageSaveContext context, CancellationToken cancellationToken = default);
}
