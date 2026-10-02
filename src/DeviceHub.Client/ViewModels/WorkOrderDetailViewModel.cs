using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Controls.Controls;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class WorkOrderDetailViewModel : ObservableObject, IDialogAware
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly IDialogService _dialogService;

        private int _orderId;

        public WorkOrderDetailViewModel(
            IWorkOrderService workOrderService,
            IDialogService dialogService)
        {
            _workOrderService = workOrderService;
            _dialogService = dialogService;
        }

        public event Action<bool>? RequestClose;

        /// <summary>
        /// 数据
        /// </summary>

        [ObservableProperty] private WorkOrderDto? _order;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string? _errorMessage;

        public ObservableCollection<WorkOrderLogDto> Logs { get; } = new();

        /// <summary>
        /// 步骤条状态 
        /// </summary>

        public bool IsPending => Order?.Status == WorkOrderStatus.Pending;
        public bool IsProcessing => Order?.Status == WorkOrderStatus.Processing;
        public bool IsClosed => Order?.Status == WorkOrderStatus.Closed;

        /// <summary>
        /// 按钮可用性
        /// </summary>

        public bool CanStart => Order?.Status == WorkOrderStatus.Pending;
        public bool CanClose => Order?.Status == WorkOrderStatus.Processing;

        /// <summary>
        /// 当前步骤索引（0=待处理，1=处理中，2=已关闭）
        /// </summary>
        public int CurrentStep
        {
            get
            {
                if (Order == null) return 0;
                return Order.Status switch
                {
                    WorkOrderStatus.Pending => 0,
                    WorkOrderStatus.Processing => 1,
                    WorkOrderStatus.Closed => 2,
                    _ => 0
                };
            }
        }
        public bool HasOrder => Order != null;

        partial void OnOrderChanged(WorkOrderDto? value)
        {
            OnPropertyChanged(nameof(HasOrder));
        }
        public async Task InitializeAsync(int orderId)
        {
            _orderId = orderId;
            await LoadAsync();
        }


        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            if (Order == null) return;

            var confirmed = _dialogService.Confirm(
                $"确认开始处理工单「{Order.Title}」？",
                "开始处理");

            if (!confirmed) return;

            try
            {
                await _workOrderService.StartAsync(Order.Id, "admin");
                await LoadAsync();
                _dialogService.ShowSuccess("工单已开始处理");
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message, "无法开始处理");
            }
        }

        [RelayCommand(CanExecute = nameof(CanClose))]
        private void Close()
        {
            if (Order == null) return;

            var id = Order.Id;
            var title = Order.Title;

            var result = _dialogService.ShowDialog<WorkOrderCloseViewModel, WorkOrderCloseView>(
                vm => vm.Initialize(id, title));

            if (result == true)
            {
                _ = LoadAsync();
                _dialogService.ShowSuccess("工单已关闭");
            }
        }

        [RelayCommand]
        private void CloseDialog() => RequestClose?.Invoke(false);


        private async Task LoadAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Order = await _workOrderService.GetByIdAsync(_orderId);

                if (Order == null)
                {
                    ErrorMessage = $"工单 {_orderId} 不存在";
                    return;
                }

                var logs = await _workOrderService.GetLogsAsync(_orderId);
                Logs.Clear();
                foreach (var l in logs) Logs.Add(l);

                // 通知状态相关属性变化
                OnPropertyChanged(nameof(HasOrder));
                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(IsPending));
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(IsClosed));
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanClose));
                StartCommand.NotifyCanExecuteChanged();
                CloseCommand.NotifyCanExecuteChanged();
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
}
