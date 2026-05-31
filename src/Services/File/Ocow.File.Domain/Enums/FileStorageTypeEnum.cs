namespace Ocow.Files.Domain.Enums;

/// <summary>
/// 文件存储类型枚举，用于区分本地存储和对象存储。
/// </summary>
public enum FileStorageTypeEnum
{
    /// <summary>
    /// 本地文件系统存储。
    /// </summary>
    Local = 1,

    /// <summary>
    /// 腾讯云 COS 存储。
    /// </summary>
    TencentCos = 2
}
