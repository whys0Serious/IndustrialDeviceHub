using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using DeviceHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
        ///  Modbus、AI等服务也在这里注册
        /// </summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceCategoryRepository, DeviceCategoryRepository>();
            return services;
        }

        /// <summary>
        /// 注册Infrastructure 层
        /// 包含EFCoreDbContext连接字符串由调用方传入
        /// WPF与Api传各自的连接字符串，但注册逻辑一致
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<DeviceHubDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
