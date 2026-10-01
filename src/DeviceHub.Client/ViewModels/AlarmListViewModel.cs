using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Messages;
using DeviceHub.Client.Services;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class AlarmListViewModel : ObservableObject
    {
        private readonly IAlarmService _alarmService;
        private readonly IDialogService _dialogService;

        public AlarmListViewModel(IAlarmService alarmService, IDialogService dialogService)
        {
            _alarmService = alarmService;
            _dialogService = dialogService;

            StatusOptions = new List<AlarmStatusOption>
        {
            new() { Display = "全部", Value = null },
            new() { Display = "未确认", Value = AlarmStatus.Unacknowledged },
            new() { Display = "已确认", Value = AlarmStatus.Acknowledged }
        };
            SelectedStatus = StatusOptions.First();
            TypeOptions = new List<AlarmTypeOption>
        {
            new() { Display = "全部", Value = null },
            new() { Display = "温度过高", Value = AlarmType.TemperatureHigh },
            new() { Display = "压力过高", Value = AlarmType.PressureHigh },
            new() { Display = "转速过低", Value = AlarmType.SpeedLow },
            new() { Display = "连接断开", Value = AlarmType.ConnectionLost }
        };
            SelectedType = TypeOptions.First();

        }

        /// <summary>
        ///筛选
        /// </summary>

        public IReadOnlyList<AlarmStatusOption> StatusOptions { get; }

        public IReadOnlyList<AlarmTypeOption> TypeOptions { get; }

        [ObservableProperty]
        private AlarmStatusOption? _selectedStatus;

        [ObservableProperty]
        private AlarmTypeOption? _selectedType;

        /// <summary>
        /// 分页
        /// </summary>

        [ObservableProperty]
        private int _currentPageIndex = 1;

        [ObservableProperty]
        private int _pageSize = 20;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private int _totalCount;

        /// <summary>
        ///状态
        /// </summary>

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        public bool HasData => Alarms.Count > 0;

        public ObservableCollection<AlarmDto> Alarms { get; } = new();

        [ObservableProperty]
        private AlarmDto? _selectedAlarm;

        private bool _isInitializing = true;

        public async Task InitializeAsync()
        {
            _isInitializing = true;
            await LoadAsync();
            _isInitializing = false;
        }

        partial void OnSelectedStatusChanged(AlarmStatusOption? value)
        {
            if (_isInitializing) return;
            CurrentPageIndex = 1;
            _ = LoadAsync();
        }

        partial void OnSelectedTypeChanged(AlarmTypeOption? value)
        {
            if (_isInitializing) return;
            CurrentPageIndex = 1;
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAsync();
        }



        [RelayCommand]
        private async Task PageUpdatedAsync()
        {
            await LoadAsync();
        }

        [RelayCommand(CanExecute = nameof(CanAcknowledge))]
        private async Task AcknowledgeAsync()
        {
            if (SelectedAlarm == null) return;

            if (SelectedAlarm.Status == AlarmStatus.Acknowledged)
            {
                _dialogService.ShowInfo("该报警已确认", "提示");
                return;
            }

            var confirmed = _dialogService.Confirm(
                $"确认报警「{SelectedAlarm.Message}」已处理？",
                "确认报警");

            if (!confirmed) return;

            try
            {
                await _alarmService.AcknowledgeAsync(SelectedAlarm.Id, "admin");
                await LoadAsync();
                //通知横幅刷新
                WeakReferenceMessenger.Default.Send(new AlarmChangedMessage());
                _dialogService.ShowSuccess("报警已确认");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"确认失败：{ex.Message}");
            }
        }

        private bool CanAcknowledge() => SelectedAlarm != null;

        partial void OnSelectedAlarmChanged(AlarmDto? value)
        {
            AcknowledgeCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var (items, total) = await _alarmService.GetPagedAsync(
                    SelectedStatus?.Value,
                    SelectedType?.Value,
                    CurrentPageIndex,
                    PageSize);

                Alarms.Clear();
                foreach (var a in items) Alarms.Add(a);

                TotalCount = total;
                TotalPages = PageSize > 0 ? (int)Math.Ceiling((double)total / PageSize) : 0;

                OnPropertyChanged(nameof(HasData));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载失败：{ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    /// <summary>
    /// 报警状态筛选选项
    /// </summary>
    public class AlarmStatusOption
    {
        public string Display { get; set; } = string.Empty;
        public AlarmStatus? Value { get; set; }
    }
    /// <summary>
    /// 报警类型筛选选项
    /// </summary>
    public class AlarmTypeOption
    {
        public string Display { get; set; } = string.Empty;
        public AlarmType? Value { get; set; }
    }
}
