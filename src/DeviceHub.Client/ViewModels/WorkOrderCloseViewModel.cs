using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Controls.Controls;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class WorkOrderCloseViewModel : ObservableObject, IDialogAware
    {
        private readonly IWorkOrderService _workOrderService;

        public WorkOrderCloseViewModel(IWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        public event Action<bool>? RequestClose;

        private int _orderId;

        [ObservableProperty] private string _title = "关闭工单";
        [ObservableProperty] private string _orderTitle = string.Empty;
        [ObservableProperty] private string _resolution = string.Empty;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string? _errorMessage;

        public void Initialize(int orderId, string orderTitle)
        {
            _orderId = orderId;
            OrderTitle = orderTitle;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Resolution))
            {
                ErrorMessage = "请填写处理结果";
                return;
            }

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                await _workOrderService.CloseAsync(_orderId, Resolution.Trim());
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
