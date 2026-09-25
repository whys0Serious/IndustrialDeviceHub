using DeviceHub.Client.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DeviceHub.Client.Extensions
{
    /// <summary>
    /// 自动扫描程序集 注册所有ViewModel
    /// 类名以ViewModel结尾的自动注册
    /// </summary>
    public static class ViewModelRegistrationExtensions
    {
        public static IServiceCollection AddViewModelsFromAssembly(
            this IServiceCollection services,
            Assembly assembly)
        {
            var viewModelTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => t.Name.EndsWith("ViewModel", StringComparison.Ordinal));

            foreach (var type in viewModelTypes)
            {
                var attr = type.GetCustomAttribute<ViewModelAttribute>();
                var lifetime = attr?.Lifetime ?? ServiceLifetime.Transient;

                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(type);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(type);
                        break;
                    case ServiceLifetime.Transient:
                    default:
                        services.AddTransient(type);
                        break;
                }
            }

            return services;
        }
    }
}
