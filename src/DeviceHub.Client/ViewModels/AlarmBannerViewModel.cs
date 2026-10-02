using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceHub.Client.Messages;
using DeviceHub.Client.Services;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Threading;

namespace DeviceHub.Client.ViewModels
{
    public partial class AlarmBannerViewModel : ObservableObject, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DispatcherTimer _timer;
        private readonly INavigationService _navigation;

        public AlarmBannerViewModel(IServiceScopeFactory scopeFactory, INavigationService navigation)
        {
            _scopeFactory = scopeFactory;
            _navigation = navigation;

            // 订阅报警变化消息：报警被确认/新增时立即刷新
            WeakReferenceMessenger.Default.Register<AlarmChangedMessage>(this, (_, _) =>
            {
                _ = RefreshAsync();
            });
            //每5秒刷新一次未确认报警
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += async (_, _) => await RefreshAsync();
        }

        /// <summary>
        /// 未确认的报警
        /// </summary>
        public ObservableCollection<AlarmDto> UnacknowledgedAlarms { get; } = new();

        /// <summary>
        /// 第一条未确认报警的消息（横幅显示）
        /// </summary>
        public string? FirstAlarmMessage =>
            UnacknowledgedAlarms.Count > 0
                ? UnacknowledgedAlarms[0].Message
                : null;
        /// <summary>
        /// 状态栏显示的文字
        /// </summary>
        public string StatusMessage => $"{UnacknowledgedCount} 条未确认报警";

        [ObservableProperty]
        private int _unacknowledgedCount;

        public bool HasAlarms => UnacknowledgedCount > 0;

        partial void OnUnacknowledgedCountChanged(int value)
        {
            OnPropertyChanged(nameof(HasAlarms));
            OnPropertyChanged(nameof(StatusMessage));
        }

        public void Start()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                _ = RefreshAsync();
            }
        }

        public void Dispose()
        {
            _timer.Stop();
            WeakReferenceMessenger.Default.Unregister<AlarmChangedMessage>(this);
        }

        private async Task RefreshAsync()
        {
            try
            {
                //每次创建 Scope，拿新的 DbContext
                using var scope = _scopeFactory.CreateScope();
                var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

                var alarms = await alarmService.GetUnacknowledgedAsync(5);

                UnacknowledgedAlarms.Clear();
                foreach (var a in alarms) UnacknowledgedAlarms.Add(a);

                UnacknowledgedCount = alarms.Count;

                //通知FirstAlarmMessage变化
                OnPropertyChanged(nameof(FirstAlarmMessage));
            }
            catch
            {
                
            }
        }

        [RelayCommand]
        private void ViewAlarms()
        {
            _navigation.NavigateTo<AlarmListViewModel>();
        }

        [RelayCommand]
        private async Task RefreshNowAsync()
        {
            await RefreshAsync();
        }
    }
}
