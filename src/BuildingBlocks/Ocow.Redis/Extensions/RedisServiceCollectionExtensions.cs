using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Redis.Interfaces;
using Ocow.Redis.Options;
using Ocow.Redis.Services;
using StackExchange.Redis;

namespace Ocow.Redis.Extensions;

/// <summary>
/// Redis 服务注册扩展。
/// </summary>
public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Redis 连接、缓存服务和分布式锁服务。
    /// </summary>
    public static IServiceCollection AddOcowRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOption>(configuration.GetSection("Redis"));
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var option = provider.GetRequiredService<IOptions<RedisOption>>().Value;
            return ConnectionMultiplexer.Connect(option.Configuration);
        });
        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IRedisLockService, RedisLockService>();

        return services;
    }
}
