using Microsoft.Extensions.Options;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Options;

namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 文件存储提供者工厂，用于根据配置选择本地存储或腾讯 COS 存储。
/// </summary>
public class FileStorageProviderFactory
{
    private readonly FileStorageOption _option;
    private readonly LocalFileStorageProvider _localProvider;
    private readonly TencentCosFileStorageProvider _tencentCosProvider;

    /// <summary>
    /// 创建文件存储提供者工厂。
    /// </summary>
    public FileStorageProviderFactory(
        IOptions<FileStorageOption> option,
        LocalFileStorageProvider localProvider,
        TencentCosFileStorageProvider tencentCosProvider)
    {
        _option = option.Value;
        _localProvider = localProvider;
        _tencentCosProvider = tencentCosProvider;
    }

    /// <summary>
    /// 根据 FileStorage:StorageType 返回当前启用的存储提供者。
    /// </summary>
    public IFileStorageProvider Create()
    {
        return NormalizeStorageType(_option.StorageType) switch
        {
            "local" => _localProvider,
            "tencentcos" or "cos" => _tencentCosProvider,
            _ => throw new InvalidOperationException($"不支持的文件存储类型：{_option.StorageType}")
        };
    }

    /// <summary>
    /// 标准化存储类型配置值。
    /// </summary>
    private static string NormalizeStorageType(string? storageType)
    {
        return storageType?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
