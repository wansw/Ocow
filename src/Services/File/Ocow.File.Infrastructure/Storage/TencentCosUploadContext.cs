namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 腾讯 COS 上传上下文，用于传递存储桶、地域、对象键和文件流。
/// </summary>
public class TencentCosUploadContext
{
    /// <summary>
    /// 腾讯 COS 存储桶名称。
    /// </summary>
    public string BucketName { get; init; } = string.Empty;

    /// <summary>
    /// 腾讯 COS 地域。
    /// </summary>
    public string Region { get; init; } = string.Empty;

    /// <summary>
    /// 腾讯 COS 对象键。
    /// </summary>
    public string ObjectKey { get; init; } = string.Empty;

    /// <summary>
    /// 待上传文件内容流。
    /// </summary>
    public Stream Content { get; init; } = Stream.Null;
}
