using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Cache.Extensions;
using Ocow.Cache.Interfaces;
using Ocow.Cache.Options;
using Ocow.Cache.Services;
using Ocow.Redis.Extensions;

namespace Ocow.Tests.Unit;

/// <summary>
/// Cache 边界测试，用于验证缓存抽象和缓存服务归属 Ocow.Cache。
/// </summary>
public class CacheBoundaryTests
{
    /// <summary>
    /// 验证缓存抽象和 Redis 缓存实现都由 Ocow.Cache 提供。
    /// </summary>
    [Fact]
    public void CacheTypes_ShouldBelongToCacheAssembly()
    {
        Assert.Equal("Ocow.Cache", typeof(ICacheService).Assembly.GetName().Name);
        Assert.Equal("Ocow.Cache", typeof(RedisCacheService).Assembly.GetName().Name);
    }

    /// <summary>
    /// 验证 AddOcowRedis 只注册 Redis 底层能力，不再注册缓存服务。
    /// </summary>
    [Fact]
    public void AddOcowRedis_ShouldNotRegisterCacheService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:Configuration"] = "localhost:6379,abortConnect=false"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddOcowRedis(configuration);

        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(ICacheService));
        Assert.DoesNotContain(services, descriptor => descriptor.ImplementationType == typeof(RedisCacheService));
    }

    /// <summary>
    /// 验证 AddOcowCache 会注册统一缓存服务。
    /// </summary>
    [Fact]
    public void AddOcowCache_ShouldRegisterCacheService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:Configuration"] = "localhost:6379,abortConnect=false",
                ["Cache:KeyPrefix"] = "ocow-test",
                ["Cache:DefaultExpireSeconds"] = "60"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddOcowCache(configuration);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ICacheService) &&
            descriptor.ImplementationType == typeof(RedisCacheService));
    }

    /// <summary>
    /// 验证未配置 Cache 节点时会兼容读取 Redis 下的旧缓存配置。
    /// </summary>
    [Fact]
    public void AddOcowCache_WhenCacheSectionMissing_ShouldUseLegacyRedisCacheSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Redis:Configuration"] = "localhost:6379,abortConnect=false",
                ["Redis:KeyPrefix"] = "legacy-prefix",
                ["Redis:DefaultExpireSeconds"] = "90"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddOcowCache(configuration);

        using var provider = services.BuildServiceProvider();
        var option = provider.GetRequiredService<IOptions<CacheOption>>().Value;

        Assert.Equal("legacy-prefix", option.KeyPrefix);
        Assert.Equal(90, option.DefaultExpireSeconds);
    }

    /// <summary>
    /// 验证 Ocow.Redis 程序集不再包含缓存抽象。
    /// </summary>
    [Fact]
    public void RedisAssembly_ShouldNotContainCacheAbstractions()
    {
        var redisAssembly = typeof(RedisServiceCollectionExtensions).Assembly;

        Assert.Null(redisAssembly.GetType("Ocow.Redis.Interfaces.IRedisCacheService"));
        Assert.Null(redisAssembly.GetType("Ocow.Redis.Services.RedisCacheService"));
    }
}
