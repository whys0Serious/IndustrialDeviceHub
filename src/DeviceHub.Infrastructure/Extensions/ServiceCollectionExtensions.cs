using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using DeviceHub.Infrastructure.Repositories;
using DeviceHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceHub.Infrastructure.Extensions
{
    /// <summary>
    /// DI扩展方法
    /// WPF客户端与Api服务端共用同一套注册
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 注册 Core层服务
        /// </summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddInfrastructure(
              this IServiceCollection services,
              IConfiguration configuration)
        {
            // EFCore
            var connStr = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("缺少连接字符串Default");

            services.AddDbContext<DeviceHubDbContext>(options =>
                options.UseSqlServer(connStr));

            // Repository
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceCategoryRepository, DeviceCategoryRepository>();

            // Service
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDeviceCategoryService, DeviceCategoryService>();

            return services;
        }
    }
}
