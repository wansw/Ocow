using Ocow.Observability.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Ocow.Tests.Unit;

/// <summary>
/// 敏感日志属性脱敏测试，用于验证密码、Token、密钥不会以结构化属性明文输出。
/// </summary>
public class SensitiveLogEventEnricherTests
{
    /// <summary>
    /// 验证敏感结构化属性会被替换为掩码。
    /// </summary>
    [Fact]
    public void Enrich_WhenPropertyContainsSensitiveName_ShouldMaskValue()
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration()
            .Enrich.With(new SensitiveLogEventEnricher())
            .WriteTo.Sink(sink)
            .CreateLogger();

        logger.Information("login {@Request}", new
        {
            UserName = "admin",
            Password = "raw-password",
            AccessToken = "raw-token",
            Nested = new
            {
                Secret = "raw-secret",
                DisplayName = "manager"
            }
        });

        var request = Assert.IsType<StructureValue>(sink.Events.Single().Properties["Request"]);
        Assert.Equal("***", GetScalarValue(request, "Password"));
        Assert.Equal("***", GetScalarValue(request, "AccessToken"));

        var nested = Assert.IsType<StructureValue>(request.Properties.Single(x => x.Name == "Nested").Value);
        Assert.Equal("***", GetScalarValue(nested, "Secret"));
        Assert.Equal("manager", GetScalarValue(nested, "DisplayName"));
    }

    /// <summary>
    /// 从结构化日志属性中读取标量值。
    /// </summary>
    private static object? GetScalarValue(StructureValue structureValue, string propertyName)
    {
        return Assert.IsType<ScalarValue>(structureValue.Properties.Single(x => x.Name == propertyName).Value).Value;
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();

        /// <summary>
        /// 收集测试中写入的日志事件。
        /// </summary>
        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
