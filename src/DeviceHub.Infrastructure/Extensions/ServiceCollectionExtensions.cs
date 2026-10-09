using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Models;
using DeviceHub.Infrastructure.BackgroundServices;
using DeviceHub.Infrastructure.Caching;
using DeviceHub.Infrastructure.Communication;
using DeviceHub.Infrastructure.Data;
using DeviceHub.Infrastructure.Repositories;
using DeviceHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

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
                options.UseSqlServer(connStr),ServiceLifetime.Transient);

            //Repository
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<IDeviceCategoryRepository, DeviceCategoryRepository>();
            services.AddScoped<IAlarmRepository, AlarmRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Service
            services.AddScoped<IDeviceService, DeviceService>();//设备信息服务
            services.AddScoped<IDeviceCategoryService, DeviceCategoryService>();//设备分类服务
            services.AddScoped<IRealtimeDataService, RealtimeDataService>();//实时数据服务
            services.AddScoped<IAlarmService, AlarmService>();//报警服务
            services.AddScoped<IWorkOrderService, WorkOrderService>();//工单服务
            services.AddScoped<IAiKnowledgeService, AiKnowledgeService>();//AI知识库服务
            services.AddScoped<IAuthService, AuthService>();//认证服务


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
            // 会话（单例）
            services.AddSingleton<ISessionService, SessionService>();

            // Modbus 后台轮询
            services.AddHostedService<ModbusPollingService>();

            // AI 配置
            services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
           

            // Semantic Kernel
            services.AddSingleton<Kernel>(sp =>
            {
                var aiOptions = configuration.GetSection(AiOptions.SectionName).Get<AiOptions>()
                    ?? throw new InvalidOperationException("缺少Ai配置");

                if (string.IsNullOrWhiteSpace(aiOptions.ApiKey))
                    throw new InvalidOperationException("缺少Ai:ApiKey");

                var builder = Kernel.CreateBuilder();

                //用 OpenAI 兼容连接器（DeepSeek 也兼容）
                builder.AddOpenAIChatCompletion(
                    modelId: aiOptions.ModelId,
                    apiKey: aiOptions.ApiKey,
                    endpoint: new Uri(aiOptions.Endpoint));

                return builder.Build();
            });

            //AI 服务
            services.AddScoped<IAiService, AiService>();

            return services;
        }
    }
}
