using Microsoft.Extensions.Options;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Files.Application.Options;
using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 腾讯 COS 文件存储提供者，用于将上传文件保存到腾讯云对象存储。
/// </summary>
public class TencentCosFileStorageProvider : IFileStorageProvider
{
    private readonly FileStorageOption _option;
    private readonly ITencentCosClient _client;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// 创建腾讯 COS 文件存储提供者。
    /// </summary>
    public TencentCosFileStorageProvider(
        IOptions<FileStorageOption> option,
        ITencentCosClient client,
        TimeProvider? timeProvider = null)
    {
        _option = option.Value;
        _client = client;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// 保存上传文件并返回腾讯 COS 对象键和存储元数据。
    /// </summary>
    public async Task<FileStorageSaveResult> SaveAsync(FileStorageSaveContext context, CancellationToken cancellationToken = default)
    {
        ValidateOption();

        var extension = NormalizeExtension(context.Extension);
        var objectKey = CreateObjectKey(extension);

        if (context.Content.CanSeek)
        {
            context.Content.Position = 0;
        }

        var uploadResult = await _client.UploadAsync(new TencentCosUploadContext
        {
            BucketName = _option.CosBucketName.Trim(),
            Region = _option.CosRegion.Trim(),
            ObjectKey = objectKey,
            Content = context.Content
        }, cancellationToken);

        return new FileStorageSaveResult
        {
            ObjectKey = objectKey,
            FileSize = uploadResult.FileSize,
            StorageType = FileStorageTypeEnum.TencentCos,
            BucketName = _option.CosBucketName.Trim(),
            Region = _option.CosRegion.Trim()
        };
    }

    /// <summary>
    /// 创建腾讯 COS 对象键。
    /// </summary>
    private string CreateObjectKey(string extension)
    {
        var now = _timeProvider.GetUtcNow();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var dateKey = $"{now:yyyy/MM/dd}/{fileName}";
        var prefix = NormalizeKeyPrefix(_option.CosKeyPrefix);

        return string.IsNullOrWhiteSpace(prefix)
            ? dateKey
            : $"{prefix}/{dateKey}";
    }

    /// <summary>
    /// 校验腾讯 COS 基础配置。
    /// </summary>
    private void ValidateOption()
    {
        if (string.IsNullOrWhiteSpace(_option.CosBucketName))
        {
            throw new InvalidOperationException("腾讯 COS BucketName 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(_option.CosRegion))
        {
            throw new InvalidOperationException("腾讯 COS Region 不能为空。");
        }
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

    /// <summary>
    /// 标准化对象键前缀。
    /// </summary>
    private static string NormalizeKeyPrefix(string? prefix)
    {
        return string.IsNullOrWhiteSpace(prefix)
            ? string.Empty
            : prefix.Trim().Trim('/');
    }
}
