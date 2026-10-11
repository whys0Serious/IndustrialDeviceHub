using DeviceHub.Client.Extensions;
using DeviceHub.Client.Themes;
using DeviceHub.Client.Views;
using DeviceHub.Controls.Controls;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace DeviceHub.Client;

public partial class App : Application
{
    private readonly IHost _host;

    public static IServiceProvider Services => ((App)Current)._host.Services;

    public App()
    {
        GlobalExceptionHandler.Register();//注册全局异常处理

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.SetBasePath(Directory.GetCurrentDirectory());
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddClientServices(ctx.Configuration);
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await authService.EnsureDefaultAdminAsync();
            }

            ThemeManager.ApplyTheme(AppTheme.Light);
            //先创建主窗口
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            //显示登录窗口
            var loginWindow = _host.Services.GetRequiredService<LoginView>();
            var loginResult = loginWindow.ShowDialog();
            if (loginResult != true)
            {
                Shutdown();
                return;
            }
            //登录成功后再显示主窗口
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动失败：{ex.Message}", "错误",
            MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
        }
        catch (OperationCanceledException)
        {
            //正常取消
        }
        finally
        {
            _host.Dispose();
        }
        base.OnExit(e);
    }
}