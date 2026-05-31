using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Application.Models;

/// <summary>
/// 文件存储保存结果，用于返回对象键、最终文件大小和真实存储位置。
/// </summary>
public class FileStorageSaveResult
{
    /// <summary>
    /// 对象键，本地存储时为相对路径。
    /// </summary>
    public string ObjectKey { get; init; } = string.Empty;

    /// <summary>
    /// 文件大小，单位字节。
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// 实际使用的文件存储类型。
    /// </summary>
    public FileStorageTypeEnum StorageType { get; init; } = FileStorageTypeEnum.Local;

    /// <summary>
    /// 对象存储 Bucket 名称，本地存储时为空。
    /// </summary>
    public string? BucketName { get; init; }

    /// <summary>
    /// 对象存储地域，本地存储时为空。
    /// </summary>
    public string? Region { get; init; }
}
