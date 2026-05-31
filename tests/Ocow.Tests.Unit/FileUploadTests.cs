using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Ocow.Auth.Attributes;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Models;
using Ocow.Files.Application.Options;
using Ocow.Files.Application.Services;
using Ocow.Files.Domain.Enums;
using Ocow.Files.Domain.Models;
using Ocow.Files.Api.Controllers.Admin;
using Ocow.Files.Api.Controllers.Client;
using Ocow.Files.Infrastructure.Storage;
using Ocow.Files.Infrastructure.Validation;

namespace Ocow.Tests.Unit;

/// <summary>
/// 文件上传测试，用于验证文件校验、本地存储和元数据保存行为。
/// </summary>
public class FileUploadTests
{
    [Fact]
    public async Task FileValidator_ShouldRejectDangerousExtension()
    {
        var validator = new FileValidator(Options.Create(new FileUploadOption()));
        await using var stream = new MemoryStream([1, 2, 3]);

        var result = await validator.ValidateAsync(new FileValidationContext
        {
            OriginalName = "bad.exe",
            MimeType = "application/octet-stream",
            Length = stream.Length,
            Content = stream
        });

        Assert.False(result.IsValid);
        Assert.Equal("FILE_EXTENSION_NOT_ALLOWED", result.Code);
    }

    [Fact]
    public async Task LocalFileStorageProvider_ShouldSaveWithDateFolderAndGuidName()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "ocow-file-tests", Guid.NewGuid().ToString("N"));
        var provider = new LocalFileStorageProvider(
            Options.Create(new FileStorageOption { LocalRootPath = rootPath }),
            new FixedTimeProvider(new DateTimeOffset(2026, 5, 31, 8, 0, 0, TimeSpan.Zero)));
        await using var stream = new MemoryStream("hello"u8.ToArray());

        var result = await provider.SaveAsync(new FileStorageSaveContext
        {
            Extension = ".txt",
            Content = stream
        });

        Assert.Matches(@"^2026/05/31/[0-9a-f]{32}\.txt$", result.ObjectKey);
        Assert.True(System.IO.File.Exists(Path.Combine(rootPath, result.ObjectKey.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal(5, result.FileSize);
        Assert.Equal(FileStorageTypeEnum.Local, result.StorageType);
        Assert.Null(result.BucketName);
        Assert.Null(result.Region);
    }

    [Fact]
    public async Task FileUploadAppService_ShouldStoreFileAndCreateMetadata()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "ocow-file-tests", Guid.NewGuid().ToString("N"));
        var repository = new InMemoryFileResourceRepository();
        var service = new FileUploadAppService(
            new FileValidator(Options.Create(new FileUploadOption())),
            new LocalFileStorageProvider(
                Options.Create(new FileStorageOption { LocalRootPath = rootPath }),
                new FixedTimeProvider(new DateTimeOffset(2026, 5, 31, 8, 0, 0, TimeSpan.Zero))),
            repository);
        await using var stream = new MemoryStream("hello"u8.ToArray());

        var result = await service.UploadAsync(new FileUploadCommand
        {
            OriginalName = "hello.txt",
            MimeType = "text/plain",
            Length = stream.Length,
            Content = stream,
            BizType = "demo",
            BizId = "1001",
            UploaderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UploaderScope = "admin"
        });

        Assert.NotEqual(Guid.Empty, result.FileId);
        Assert.Equal("hello.txt", result.FileName);
        Assert.Equal("txt", result.FileCategory);
        Assert.Equal(5, result.FileSize);
        var saved = Assert.Single(repository.Resources);
        Assert.Equal(result.FileId, saved.Id);
        Assert.Equal("hello.txt", saved.OriginalName);
        Assert.Equal("demo", saved.BizType);
        Assert.Equal("1001", saved.BizId);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), saved.UploaderId);
        Assert.True(System.IO.File.Exists(Path.Combine(rootPath, saved.ObjectKey.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task FileUploadAppService_ShouldStoreStorageMetadataFromProvider()
    {
        var repository = new InMemoryFileResourceRepository();
        var service = new FileUploadAppService(
            new FileValidator(Options.Create(new FileUploadOption())),
            new StubFileStorageProvider(new FileStorageSaveResult
            {
                ObjectKey = "biz/2026/05/31/file.txt",
                FileSize = 5,
                StorageType = FileStorageTypeEnum.TencentCos,
                BucketName = "ocow-test-1250000000",
                Region = "ap-guangzhou"
            }),
            repository);
        await using var stream = new MemoryStream("hello"u8.ToArray());

        await service.UploadAsync(new FileUploadCommand
        {
            OriginalName = "hello.txt",
            MimeType = "text/plain",
            Length = stream.Length,
            Content = stream
        });

        var saved = Assert.Single(repository.Resources);
        Assert.Equal(FileStorageTypeEnum.TencentCos, saved.StorageType);
        Assert.Equal("ocow-test-1250000000", saved.BucketName);
        Assert.Equal("ap-guangzhou", saved.Region);
        Assert.Equal("biz/2026/05/31/file.txt", saved.ObjectKey);
    }

    [Theory]
    [InlineData("Local", typeof(LocalFileStorageProvider))]
    [InlineData("TencentCos", typeof(TencentCosFileStorageProvider))]
    [InlineData("Cos", typeof(TencentCosFileStorageProvider))]
    public void FileStorageProviderFactory_ShouldResolveProviderByStorageType(string storageType, Type expectedType)
    {
        var option = Options.Create(new FileStorageOption
        {
            StorageType = storageType,
            LocalRootPath = ".appdata/files",
            CosBucketName = "ocow-test-1250000000",
            CosRegion = "ap-guangzhou",
            CosSecretId = "secret-id",
            CosSecretKey = "secret-key"
        });
        var localProvider = new LocalFileStorageProvider(option);
        var cosProvider = new TencentCosFileStorageProvider(option, new FakeTencentCosClient(), TimeProvider.System);
        var factory = new FileStorageProviderFactory(option, localProvider, cosProvider);

        var provider = factory.Create();

        Assert.IsType(expectedType, provider);
    }

    [Fact]
    public async Task TencentCosFileStorageProvider_ShouldUploadWithConfiguredBucketPrefixAndMetadata()
    {
        var client = new FakeTencentCosClient();
        var provider = new TencentCosFileStorageProvider(
            Options.Create(new FileStorageOption
            {
                CosBucketName = "ocow-test-1250000000",
                CosRegion = "ap-guangzhou",
                CosSecretId = "secret-id",
                CosSecretKey = "secret-key",
                CosKeyPrefix = "biz/uploads"
            }),
            client,
            new FixedTimeProvider(new DateTimeOffset(2026, 5, 31, 8, 0, 0, TimeSpan.Zero)));
        await using var stream = new MemoryStream("hello"u8.ToArray());

        var result = await provider.SaveAsync(new FileStorageSaveContext
        {
            Extension = ".TXT",
            Content = stream
        });

        Assert.Equal(FileStorageTypeEnum.TencentCos, result.StorageType);
        Assert.Equal("ocow-test-1250000000", result.BucketName);
        Assert.Equal("ap-guangzhou", result.Region);
        Assert.Equal(5, result.FileSize);
        Assert.Matches(@"^biz/uploads/2026/05/31/[0-9a-f]{32}\.txt$", result.ObjectKey);
        Assert.NotNull(client.Context);
        Assert.Equal("ocow-test-1250000000", client.Context.BucketName);
        Assert.Equal("ap-guangzhou", client.Context.Region);
        Assert.Equal(result.ObjectKey, client.Context.ObjectKey);
    }

    [Fact]
    public void FilesController_ShouldExposeClientUploadRoute()
    {
        var route = Assert.IsType<RouteAttribute>(Attribute.GetCustomAttribute(typeof(FilesController), typeof(RouteAttribute)));
        var method = typeof(FilesController).GetMethod(nameof(FilesController.UploadAsync));
        var httpPost = Assert.IsType<HttpPostAttribute>(method!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Single());

        Assert.Equal("api/files", route.Template);
        Assert.Equal("upload", httpPost.Template);
    }

    [Fact]
    public void AdminFilesController_ShouldExposeAdminUploadRouteWithPermission()
    {
        var route = Assert.IsType<RouteAttribute>(Attribute.GetCustomAttribute(typeof(AdminFilesController), typeof(RouteAttribute)));
        var method = typeof(AdminFilesController).GetMethod(nameof(AdminFilesController.UploadAsync));
        var httpPost = Assert.IsType<HttpPostAttribute>(method!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Single());
        var permission = Assert.IsType<PermissionAuthorizeAttribute>(method.GetCustomAttributes(typeof(PermissionAuthorizeAttribute), inherit: false).Single());

        Assert.Equal("api/admin/files", route.Template);
        Assert.Equal("upload", httpPost.Template);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}file.upload", permission.Policy);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class InMemoryFileResourceRepository : IFileResourceRepository
    {
        public List<FileResource> Resources { get; } = new();

        public Task AddAsync(FileResource resource, CancellationToken cancellationToken = default)
        {
            Resources.Add(resource);
            return Task.CompletedTask;
        }
    }

    private sealed class StubFileStorageProvider : IFileStorageProvider
    {
        private readonly FileStorageSaveResult _result;

        public StubFileStorageProvider(FileStorageSaveResult result)
        {
            _result = result;
        }

        public Task<FileStorageSaveResult> SaveAsync(FileStorageSaveContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeTencentCosClient : ITencentCosClient
    {
        public TencentCosUploadContext? Context { get; private set; }

        public async Task<TencentCosUploadResult> UploadAsync(TencentCosUploadContext context, CancellationToken cancellationToken = default)
        {
            Context = context;
            if (context.Content.CanSeek)
            {
                context.Content.Position = 0;
            }

            await using var copied = new MemoryStream();
            await context.Content.CopyToAsync(copied, cancellationToken);

            return new TencentCosUploadResult
            {
                FileSize = copied.Length
            };
        }
    }
}
