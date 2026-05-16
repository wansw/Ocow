namespace Ocow.Cache.Constants;

/// <summary>
/// 缓存键生成器，用于统一缓存键拼接规则。
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// 根据缓存前缀和业务键生成最终缓存键。
    /// </summary>
    public static string Build(string prefix, string key)
    {
        var normalizedPrefix = prefix.Trim(':');
        var normalizedKey = key.TrimStart(':');
        return string.IsNullOrWhiteSpace(normalizedPrefix) ? normalizedKey : $"{normalizedPrefix}:{normalizedKey}";
    }

    /// <summary>
    /// 按冒号拼接多个缓存键片段。
    /// </summary>
    public static string Join(params string[] parts)
    {
        return string.Join(":", parts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim(':')));
    }
}
