using Microsoft.Extensions.Options;
using Ocow.InternalAuth.Options;
using Ocow.InternalAuth.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// HMAC 签名单元测试，用于验证内部服务签名生成和校验。/// </summary>
public class HmacSignatureTests
{
    /// <summary>
    /// 验证同一请求信息生成的签名可以通过校验。    /// </summary>
    [Fact]
    public void Validate_WhenSignatureMatches_ShouldReturnTrue()
    {
        var service = new HmacSignatureService(Options.Create(new HmacSignatureOption
        {
            Secret = "UnitTestHmacSecret",
            TimestampToleranceSeconds = 300
        }));
        var timestamp = DateTimeOffset.UtcNow.ToString("O");
        var signature = service.Generate("Ocow.Scheduler", "POST", "/internal/orders/sync/erp", timestamp, "nonce-1", string.Empty);

        var result = service.Validate("Ocow.Scheduler", "POST", "/internal/orders/sync/erp", timestamp, "nonce-1", string.Empty, signature);

        Assert.True(result);
    }

    /// <summary>
    /// 验证请求路径被篡改时签名校验失败。    /// </summary>
    [Fact]
    public void Validate_WhenPathChanged_ShouldReturnFalse()
    {
        var service = new HmacSignatureService(Options.Create(new HmacSignatureOption
        {
            Secret = "UnitTestHmacSecret",
            TimestampToleranceSeconds = 300
        }));
        var timestamp = DateTimeOffset.UtcNow.ToString("O");
        var signature = service.Generate("Ocow.Scheduler", "POST", "/internal/orders/sync/erp", timestamp, "nonce-1", string.Empty);

        var result = service.Validate("Ocow.Scheduler", "POST", "/internal/orders/other", timestamp, "nonce-1", string.Empty, signature);

        Assert.False(result);
    }
}
