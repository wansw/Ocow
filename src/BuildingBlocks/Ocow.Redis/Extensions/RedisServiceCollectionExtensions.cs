using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    /// 注册 Redis 连接、分布式锁服务和限流服务。
    /// </summary>
    public static IServiceCollection AddOcowRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOption>(configuration.GetSection("Redis"));
        services.TryAddSingleton<IConnectionMultiplexer>(provider =>
        {
            var option = provider.GetRequiredService<IOptions<RedisOption>>().Value;
            var redisConfiguration = ConfigurationOptions.Parse(option.Configuration);
            redisConfiguration.AbortOnConnectFail = false;
            if (option.DefaultDatabase.HasValue)
            {
                redisConfiguration.DefaultDatabase = option.DefaultDatabase;
            }

            return ConnectionMultiplexer.Connect(redisConfiguration);
        });

        services.TryAddSingleton<IRedisLockService, RedisLockService>();
        services.TryAddSingleton<IRedisRateLimiter, RedisRateLimiter>();

        return services;
    }
}
