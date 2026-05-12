using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocow.EntityFrameworkCore.Extensions
{
    public static class RepositoryServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 EF Core DbContext、Unit of Work 和默认拦截器。
        /// </summary>
        public static IServiceCollection AddOcowDbContext<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration, Action<IServiceCollection>? configureRepositories = null)
            where TDbContext : DbContext
        {

            var section = configuration.GetSection("Database");
            var option = section.Get<DatabaseOption>() ?? new DatabaseOption(); 
            services.Configure<DatabaseOption>(section.Bind);
            services.AddOcowEntityFrameworkCore();

            services.AddDbContext<TDbContext>(builder => builder
                .UseOcowDatabase(option)
                .AddOcowSaveChangesInterceptors());

            services.AddOcowUnitOfWork<TDbContext>();

            configureRepositories?.Invoke(services);

            return services;
        }
    }
}
