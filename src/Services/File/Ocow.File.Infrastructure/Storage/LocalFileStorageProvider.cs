using Microsoft.Extensions.Options;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Files.Application.Options;
using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 本地文件存储提供者，用于将上传文件保存到本地文件系统。
/// </summary>
public class LocalFileStorageProvider : IFileStorageProvider
{
    private readonly FileStorageOption _option;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// 创建本地文件存储提供者。
    /// </summary>
    public LocalFileStorageProvider(IOptions<FileStorageOption> option, TimeProvider? timeProvider = null)
    {
        _option = option.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// 保存上传文件并返回本地相对对象键。
    /// </summary>
    public async Task<FileStorageSaveResult> SaveAsync(FileStorageSaveContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_option.LocalRootPath))
        {
            throw new InvalidOperationException("本地文件存储根目录不能为空。");
        }

        var extension = NormalizeExtension(context.Extension);
        var now = _timeProvider.GetUtcNow();
        var folderKey = $"{now:yyyy/MM/dd}";
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var objectKey = $"{folderKey}/{fileName}";

        var rootPath = Path.GetFullPath(_option.LocalRootPath);
        var folderPath = Path.Combine(rootPath, now.ToString("yyyy"), now.ToString("MM"), now.ToString("dd"));
        Directory.CreateDirectory(folderPath);

        var filePath = Path.GetFullPath(Path.Combine(folderPath, fileName));
        if (!filePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("文件存储路径非法。");
        }

        if (context.Content.CanSeek)
        {
            context.Content.Position = 0;
        }

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await context.Content.CopyToAsync(fileStream, cancellationToken);

        return new FileStorageSaveResult
        {
            ObjectKey = objectKey,
            FileSize = fileStream.Length,
            StorageType = FileStorageTypeEnum.Local
        };
    }

    /// <summary>
    /// 标准化扩展名。
    /// </summary>
    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new InvalidOperationException("文件扩展名不能为空。");
        }

        var normalized = extension.StartsWith('.') ? extension : $".{extension}";
        return normalized.ToLowerInvariant();
    }
}
