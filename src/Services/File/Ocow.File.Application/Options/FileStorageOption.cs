namespace Ocow.Files.Application.Options;

/// <summary>
/// 文件存储配置，用于控制存储方式、本地路径、腾讯 COS 参数和下载签名有效期。
/// </summary>
public class FileStorageOption
{
    /// <summary>
    /// 配置节点名称。
    /// </summary>
    public const string SectionName = "FileStorage";

    /// <summary>
    /// 存储类型，支持 Local、TencentCos 或 Cos。
    /// </summary>
    public string StorageType { get; set; } = "Local";

    /// <summary>
    /// 本地文件存储根目录。
    /// </summary>
    public string LocalRootPath { get; set; } = ".appdata/files";

    /// <summary>
    /// 腾讯 COS 存储桶名称，格式通常为 bucketname-appid。
    /// </summary>
    public string CosBucketName { get; set; } = string.Empty;

    /// <summary>
    /// 腾讯 COS 地域，例如 ap-guangzhou。
    /// </summary>
    public string CosRegion { get; set; } = string.Empty;

    /// <summary>
    /// 腾讯云 API 密钥 SecretId，用于 COS 请求签名。
    /// </summary>
    public string CosSecretId { get; set; } = string.Empty;

    /// <summary>
    /// 腾讯云 API 密钥 SecretKey，用于 COS 请求签名。
    /// </summary>
    public string CosSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 腾讯 COS 对象键前缀，用于隔离本系统上传文件目录。
    /// </summary>
    public string CosKeyPrefix { get; set; } = "uploads";

    /// <summary>
    /// 腾讯 COS 请求是否使用 HTTPS。
    /// </summary>
    public bool CosUseHttps { get; set; } = true;

    /// <summary>
    /// 腾讯 COS 单次请求签名有效秒数。
    /// </summary>
    public long CosCredentialDurationSeconds { get; set; } = 600;

    /// <summary>
    /// 下载链接有效秒数。
    /// </summary>
    public int DownloadTokenExpireSeconds { get; set; } = 300;

    /// <summary>
    /// 下载签名密钥。
    /// </summary>
    public string DownloadTokenSecret { get; set; } = "PleaseChangeThisFileDownloadTokenSecret";
}
