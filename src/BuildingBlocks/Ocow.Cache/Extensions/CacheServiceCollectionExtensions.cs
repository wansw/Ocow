using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocow.Cache.Interfaces;
using Ocow.Cache.Options;
using Ocow.Cache.Services;
using Ocow.Redis.Extensions;

namespace Ocow.Cache.Extensions;

/// <summary>
/// 缓存服务注册扩展。
/// </summary>
public static class CacheServiceCollectionExtensions
{
    /// <summary>
    /// 注册缓存抽象、Redis 缓存实现和所需的 Redis 底层连接。
    /// </summary>
    public static IServiceCollection AddOcowCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOcowRedis(configuration);
        services.Configure<CacheOption>(option =>
        {
            var cacheSection = configuration.GetSection("Cache");
            if (cacheSection.Exists())
            {
                cacheSection.Bind(option);
                return;
            }

            var redisSection = configuration.GetSection("Redis");
            var legacyKeyPrefix = redisSection["KeyPrefix"];
            if (!string.IsNullOrWhiteSpace(legacyKeyPrefix))
            {
                option.KeyPrefix = legacyKeyPrefix;
            }

            if (int.TryParse(redisSection["DefaultExpireSeconds"], out var legacyDefaultExpireSeconds))
            {
                option.DefaultExpireSeconds = legacyDefaultExpireSeconds;
            }
        });

        services.TryAddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
