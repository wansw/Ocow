namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 腾讯 COS 上传结果，用于返回已上传文件大小。
/// </summary>
public class TencentCosUploadResult
{
    /// <summary>
    /// 已上传文件大小，单位字节。
    /// </summary>
    public long FileSize { get; init; }
}
