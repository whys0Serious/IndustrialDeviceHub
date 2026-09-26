using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Threading;

namespace DeviceHub.Client.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly DispatcherTimer _timer;

        public MainWindowViewModel(INavigationService navigation)
        {
            _navigation = navigation;

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
        /// 当前显示的ViewMode（ContentControl 绑定它）
        /// </summary>
        public ObservableObject? CurrentViewModel => _navigation.CurrentViewModel;

        /// <summary>
        /// 导航到设备管理
        /// </summary>
        [RelayCommand]
        private void NavigateToDeviceList() => _navigation.NavigateTo<DeviceListViewModel>();

        /// <summary>
        /// 启动时默认导航到设备管理
        /// </summary>
        public void Initialize() => _navigation.NavigateTo<DeviceListViewModel>();

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

        // ---------- 预留：M3 加 Modbus 状态 ----------
        // [ObservableProperty] private bool _isModbusConnected;
        // [ObservableProperty] private string _modbusStatusText = "Modbus 未连接";

        // ---------- 预留：M6 加 AI 服务状态 ----------
        // [ObservableProperty] private bool _isAiAvailable;
        // [ObservableProperty] private string _aiStatusText = "AI 服务不可用";

        // ---------- 预留：M4 加后台任务进度 ----------
        // [ObservableProperty] private bool _isTaskRunning;
        // [ObservableProperty] private string _taskDescription;
        // [ObservableProperty] private int _taskProgress;

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
