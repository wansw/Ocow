namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 腾讯 COS 客户端抽象，用于隔离 SDK 调用并方便单元测试。
/// </summary>
public interface ITencentCosClient
{
    /// <summary>
    /// 上传文件流到腾讯 COS。
    /// </summary>
    Task<TencentCosUploadResult> UploadAsync(TencentCosUploadContext context, CancellationToken cancellationToken = default);
}
