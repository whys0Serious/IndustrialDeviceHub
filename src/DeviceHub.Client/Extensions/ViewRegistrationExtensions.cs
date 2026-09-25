using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace DeviceHub.Client.Extensions
{
    /// <summary>
    /// 自动扫描程序集，注册所有View（Window和UserControl）
    /// MainWindow注册为单例其他为瞬态
    /// </summary>
    public static class ViewRegistrationExtensions
    {
        public static IServiceCollection AddViewsFromAssembly(
            this IServiceCollection services,
            Assembly assembly)
        {
            var viewTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Where(t => typeof(Window).IsAssignableFrom(t)
                         || typeof(UserControl).IsAssignableFrom(t));

            foreach (var type in viewTypes)
            {
                if (type.Name == "MainWindow")
                {
                    services.AddSingleton(type);
                }
                else
                {
                    services.AddTransient(type);
                }
            }

            return services;
        }
    }
}
