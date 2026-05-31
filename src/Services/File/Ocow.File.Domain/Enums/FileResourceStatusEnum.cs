namespace Ocow.Files.Domain.Enums;

/// <summary>
/// 文件资源状态枚举，用于标识文件是否可用。
/// </summary>
public enum FileResourceStatusEnum
{
    /// <summary>
    /// 正常可用。
    /// </summary>
    Normal = 1,

    /// <summary>
    /// 已删除。
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// 已过期。
    /// </summary>
    Expired = 3
}
