namespace Ocow.HealthChecks.Constants;

/// <summary>
/// 健康检查标签常量，用于区分 live、ready 等检查范围。
/// </summary>
public static class HealthCheckTags
{
    /// <summary>
    /// 就绪检查标签，只有带该标签的依赖会进入 /ready。
    /// </summary>
    public const string Ready = "ready";
}
