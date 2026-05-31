using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ocow.Files.Api.Dtos;

/// <summary>
/// 上传文件请求 DTO，用于接收 multipart/form-data 文件和业务关联参数。
/// </summary>
public class UploadFileReqDto
{
    /// <summary>
    /// 上传文件。
    /// </summary>
    [Required(ErrorMessage = "上传文件不能为空")]
    public IFormFile File { get; init; } = default!;

    /// <summary>
    /// 业务类型。
    /// </summary>
    public string? BizType { get; init; }

    /// <summary>
    /// 业务 ID。
    /// </summary>
    public string? BizId { get; init; }
}
