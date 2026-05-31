using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using Microsoft.Extensions.Options;
using Ocow.Files.Application.Options;

namespace Ocow.Files.Infrastructure.Storage;

/// <summary>
/// 腾讯 COS SDK 客户端，用于执行真实的对象上传请求。
/// </summary>
public class TencentCosClient : ITencentCosClient
{
    private readonly FileStorageOption _option;

    /// <summary>
    /// 创建腾讯 COS SDK 客户端。
    /// </summary>
    public TencentCosClient(IOptions<FileStorageOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 上传文件流到腾讯 COS，并返回上传大小。
    /// </summary>
    public async Task<TencentCosUploadResult> UploadAsync(TencentCosUploadContext context, CancellationToken cancellationToken = default)
    {
        ValidateCosCredential();
        ValidateUploadContext(context);

        if (context.Content.CanSeek)
        {
            context.Content.Position = 0;
        }
        else
        {
            throw new InvalidOperationException("腾讯 COS 上传需要可定位的文件流。");
        }

        var sendLength = context.Content.Length - context.Content.Position;
        var request = new PutObjectRequest(
            context.BucketName,
            context.ObjectKey,
            context.Content,
            context.Content.Position,
            sendLength);
        var cosXml = CreateCosXml();

        await Task.Run(() => cosXml.PutObject(request), cancellationToken);

        return new TencentCosUploadResult
        {
            FileSize = sendLength
        };
    }

    /// <summary>
    /// 创建腾讯 COS SDK 实例。
    /// </summary>
    private CosXml CreateCosXml()
    {
        var configBuilder = new CosXmlConfig.Builder()
            .SetRegion(_option.CosRegion);
        configBuilder.IsHttps(_option.CosUseHttps);

        var config = configBuilder.Build();
        QCloudCredentialProvider credentialProvider = new DefaultQCloudCredentialProvider(
            _option.CosSecretId,
            _option.CosSecretKey,
            _option.CosCredentialDurationSeconds);

        return new CosXmlServer(config, credentialProvider);
    }

    /// <summary>
    /// 校验腾讯 COS 密钥配置。
    /// </summary>
    private void ValidateCosCredential()
    {
        if (string.IsNullOrWhiteSpace(_option.CosSecretId))
        {
            throw new InvalidOperationException("腾讯 COS SecretId 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(_option.CosSecretKey))
        {
            throw new InvalidOperationException("腾讯 COS SecretKey 不能为空。");
        }
    }

    /// <summary>
    /// 校验腾讯 COS 上传上下文。
    /// </summary>
    private static void ValidateUploadContext(TencentCosUploadContext context)
    {
        if (string.IsNullOrWhiteSpace(context.BucketName))
        {
            throw new InvalidOperationException("腾讯 COS BucketName 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(context.Region))
        {
            throw new InvalidOperationException("腾讯 COS Region 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(context.ObjectKey))
        {
            throw new InvalidOperationException("腾讯 COS ObjectKey 不能为空。");
        }
    }
}
