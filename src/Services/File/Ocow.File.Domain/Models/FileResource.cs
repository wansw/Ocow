using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocow.Files.Domain.Enums;

namespace Ocow.Files.Domain.Models;

/// <summary>
/// 文件资源实体，用于保存上传文件的存储位置和业务元数据。
/// </summary>
[Table("file_resources")]
public class FileResource
{
    /// <summary>
    /// 文件资源主键。
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 用户上传时的原始文件名。
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// 存储对象键，本地存储时为相对路径，COS 存储时为 object key。
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ObjectKey { get; set; } = string.Empty;

    /// <summary>
    /// 对象存储 Bucket 名称，本地存储为空。
    /// </summary>
    [MaxLength(100)]
    public string? BucketName { get; set; }

    /// <summary>
    /// 对象存储区域，本地存储为空。
    /// </summary>
    [MaxLength(50)]
    public string? Region { get; set; }

    /// <summary>
    /// 文件分类。
    /// </summary>
    public FileCategoryEnum FileCategory { get; set; }

    /// <summary>
    /// 浏览器上传的 MIME Type。
    /// </summary>
    [MaxLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// 文件扩展名。
    /// </summary>
    [MaxLength(20)]
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小，单位字节。
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 文件存储类型。
    /// </summary>
    public FileStorageTypeEnum StorageType { get; set; } = FileStorageTypeEnum.Local;

    /// <summary>
    /// 业务类型，用于关联调用方业务场景。
    /// </summary>
    [MaxLength(50)]
    public string? BizType { get; set; }

    /// <summary>
    /// 业务 ID，用于关联调用方业务数据。
    /// </summary>
    [MaxLength(64)]
    public string? BizId { get; set; }

    /// <summary>
    /// 上传人 ID。
    /// </summary>
    public Guid? UploaderId { get; set; }

    /// <summary>
    /// 上传人身份范围，例如 client、admin 或 internal。
    /// </summary>
    [MaxLength(20)]
    public string? UploaderScope { get; set; }

    /// <summary>
    /// 文件状态。
    /// </summary>
    public FileResourceStatusEnum Status { get; set; } = FileResourceStatusEnum.Normal;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
