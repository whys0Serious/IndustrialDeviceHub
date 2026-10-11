using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Services;
using DeviceHub.Client.Themes;
using DeviceHub.Client.Views;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Singleton)]
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly DispatcherTimer _timer;

        public AlarmBannerViewModel AlarmBanner { get; }

        private readonly ISessionService _session;

        public MainWindowViewModel(INavigationService navigation, AlarmBannerViewModel alarmBanner, ISessionService session)
        {
            _navigation = navigation;
            AlarmBanner = alarmBanner;
            _session = session;

            // 把NavigationService的属性变化转发到CurrentViewModel
            _navigation.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                    OnPropertyChanged(nameof(CurrentViewModel));
            };
            // 每秒刷新时间
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, _) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _timer.Start();

            // 读版本号
            LoadVersion();
        }

        /// <summary>
        /// 当前用户名
        /// </summary>
        public string CurrentUserName => _session.CurrentUser?.DisplayName ?? "未登录";

        /// <summary>
        /// 当前用户角色
        /// </summary>
        public string CurrentUserRole => _session.CurrentUser?.RoleText ?? "";

        /// <summary>
        /// 是否管理员
        /// </summary>
        public bool IsAdmin => _session.IsAdmin;
        /// <summary>
        /// 启动时默认导航到设备管理
        /// </summary>
        public void Initialize()
        {
            // 启动报警横幅定时器
            AlarmBanner.Start();

            // 默认导航
            _navigation.NavigateTo<DeviceListViewModel>();
        }

        /// <summary>
        /// 当前显示的ViewMode（ContentControl 绑定它）
        /// </summary>
        public ObservableObject? CurrentViewModel => _navigation.CurrentViewModel;

        /// <summary>
        /// 通用导航 参数是ViewModel类型
        /// </summary>
        /// <param name="viewModelType"></param>
        [RelayCommand]
        private void Navigate(Type? viewModelType)
        {
            if (viewModelType == null) return;
            _navigation.NavigateTo(viewModelType);
        }
        /// <summary>
        /// 切换主题（浅色/深色）
        /// </summary>
        [RelayCommand]
        private void ToggleTheme()
        {
            ThemeManager.ToggleTheme();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        [RelayCommand]
        private void Logout()
        {
            _session.Clear();

            // 主窗口不关、不隐藏，保持在后台
            var loginWindow = App.Services.GetRequiredService<LoginView>();
            var loginResult = loginWindow.ShowDialog();

            if (loginResult == true)
            {
                // 登录成功 → 刷新主窗口上的用户信息
                RefreshUserInfo();
            }
            else
            {
                // 用户取消 → 直接回到主界面
            }
        }
        public void RefreshUserInfo()
        {
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
            OnPropertyChanged(nameof(IsAdmin));
        }

        /// <summary>
        /// 软件版本号（从程序集读）
        /// </summary>
        [ObservableProperty] private string _shortVersion = "v0.0.0";
        [ObservableProperty] private string _fullVersion = "v0.0.0";

        /// <summary>
        /// 当前时间（每秒刷新）
        /// </summary>
        [ObservableProperty]
        private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        private void LoadVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var informational = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "0.0.0";

            FullVersion = $"v{informational}";

            var plusIndex = informational.IndexOf('+');
            ShortVersion = plusIndex >= 0
                ? $"v{informational.Substring(0, plusIndex)}"
                : $"v{informational}";
        }
    }
}
