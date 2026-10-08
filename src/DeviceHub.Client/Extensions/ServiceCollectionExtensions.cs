using DeviceHub.Client.Services;
using DeviceHub.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using NavigationService = DeviceHub.Client.Services.NavigationService;

namespace DeviceHub.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Client层所有服务统一注册入口
        /// App.xaml.cs只需调这一个方法
        /// </summary>
        public static IServiceCollection AddClientServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            //Core + Infrastructure
            services.AddCoreServices();
            services.AddInfrastructure(configuration);

            //导航、弹出框
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            //自动注册ViewModel和View
            services.AddViewModelsFromAssembly(assembly);
            services.AddViewsFromAssembly(assembly);

            // API地址配置
            var apiBaseUrl = configuration["Api:BaseUrl"]
                ?? throw new InvalidOperationException("缺少 Api:BaseUrl 配置");

            // HttpClient+AiClientService
            services.AddHttpClient<IAiClientService, AiClientService>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(60);  
            });
            return services;
        }
    }
}
