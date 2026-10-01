using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using DeviceHub.Infrastructure.Caching;
using DeviceHub.Infrastructure.Communication;
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

            //Repository
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceCategoryRepository, DeviceCategoryRepository>();
            services.AddScoped<IAlarmRepository, AlarmRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

            // Service
            services.AddScoped<IDeviceService, DeviceService>();//设备信息服务
            services.AddScoped<IDeviceCategoryService, DeviceCategoryService>();//设备分类服务
            services.AddScoped<IRealtimeDataService, RealtimeDataService>();//实时数据服务
            services.AddScoped<IAlarmService, AlarmService>();//报警服务
            services.AddScoped<IWorkOrderService, WorkOrderService>();//工单服务

            // 通信
            services.AddSingleton<IDeviceCommunication, ModbusDeviceCommunication>();

            // Modbus 配置
            services.Configure<ModbusOptions>(configuration.GetSection(ModbusOptions.SectionName));
            // 报警阈值配置
            services.Configure<AlarmOptions>(configuration.GetSection(AlarmOptions.SectionName));

            // 实时数据缓存（单例，线程安全）
            services.AddSingleton<IRealtimeDataCache, RealtimeDataCache>();
            // 报警状态跟踪
            services.AddSingleton<AlarmTracker>();

            // Modbus 后台轮询
            services.AddHostedService<ModbusPollingService>();

            return services;
        }
    }
}
