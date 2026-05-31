using Ocow.Files.Application.Dtos;
using Ocow.Files.Application.Models;

namespace Ocow.Files.Application.Interfaces;

/// <summary>
/// 文件上传应用服务接口，用于编排校验、存储和元数据保存。
/// </summary>
public interface IFileUploadAppService
{
    /// <summary>
    /// 上传文件并保存文件资源元数据。
    /// </summary>
    Task<UploadFileResDto> UploadAsync(FileUploadCommand command, CancellationToken cancellationToken = default);
}
