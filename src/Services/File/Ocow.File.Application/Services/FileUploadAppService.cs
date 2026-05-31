using Ocow.Files.Application.Dtos;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Files.Domain.Enums;
using Ocow.Files.Domain.Models;

namespace Ocow.Files.Application.Services;

/// <summary>
/// 文件上传应用服务，用于编排文件校验、文件存储和元数据保存。
/// </summary>
public class FileUploadAppService : IFileUploadAppService
{
    private readonly IFileValidator _fileValidator;
    private readonly IFileStorageProvider _storageProvider;
    private readonly IFileResourceRepository _fileResourceRepository;

    /// <summary>
    /// 创建文件上传应用服务。
    /// </summary>
    public FileUploadAppService(
        IFileValidator fileValidator,
        IFileStorageProvider storageProvider,
        IFileResourceRepository fileResourceRepository)
    {
        _fileValidator = fileValidator;
        _storageProvider = storageProvider;
        _fileResourceRepository = fileResourceRepository;
    }

    /// <summary>
    /// 上传文件并保存文件资源元数据。
    /// </summary>
    public async Task<UploadFileResDto> UploadAsync(FileUploadCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _fileValidator.ValidateAsync(new FileValidationContext
        {
            OriginalName = command.OriginalName,
            MimeType = command.MimeType,
            Length = command.Length,
            Content = command.Content
        }, cancellationToken);

        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Message);
        }

        if (command.Content.CanSeek)
        {
            command.Content.Position = 0;
        }

        var storageResult = await _storageProvider.SaveAsync(new FileStorageSaveContext
        {
            Extension = validation.Extension,
            Content = command.Content
        }, cancellationToken);

        var resource = new FileResource
        {
            Id = Guid.NewGuid(),
            OriginalName = command.OriginalName,
            ObjectKey = storageResult.ObjectKey,
            FileCategory = validation.FileCategory,
            MimeType = command.MimeType,
            Extension = validation.Extension,
            FileSize = storageResult.FileSize,
            StorageType = storageResult.StorageType,
            BucketName = storageResult.BucketName,
            Region = storageResult.Region,
            BizType = string.IsNullOrWhiteSpace(command.BizType) ? null : command.BizType.Trim(),
            BizId = string.IsNullOrWhiteSpace(command.BizId) ? null : command.BizId.Trim(),
            UploaderId = command.UploaderId,
            UploaderScope = string.IsNullOrWhiteSpace(command.UploaderScope) ? null : command.UploaderScope.Trim(),
            Status = FileResourceStatusEnum.Normal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _fileResourceRepository.AddAsync(resource, cancellationToken);

        return new UploadFileResDto
        {
            FileId = resource.Id,
            FileName = resource.OriginalName,
            FileCategory = ToResponseCategory(resource.FileCategory),
            FileSize = resource.FileSize
        };
    }

    /// <summary>
    /// 将文件分类转换为接口响应字符串。
    /// </summary>
    private static string ToResponseCategory(FileCategoryEnum fileCategory)
    {
        return fileCategory switch
        {
            FileCategoryEnum.Txt => "txt",
            FileCategoryEnum.Excel => "excel",
            FileCategoryEnum.Image => "image",
            _ => "unknown"
        };
    }
}
