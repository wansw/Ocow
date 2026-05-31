using Ocow.Files.Application.Models;

namespace Ocow.Files.Application.Interfaces;

/// <summary>
/// 文件校验器接口，用于校验上传文件大小、扩展名和真实格式。
/// </summary>
public interface IFileValidator
{
    /// <summary>
    /// 校验上传文件是否允许保存。
    /// </summary>
    Task<FileValidationResult> ValidateAsync(FileValidationContext context, CancellationToken cancellationToken = default);
}
