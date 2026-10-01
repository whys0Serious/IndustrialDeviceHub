using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Controls;
using DeviceHub.Client.Models;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class WorkOrderEditViewModel : ObservableObject, IDialogAware
    {
        public event Action<bool>? RequestClose;
        private readonly IWorkOrderService _workOrderService;
        private readonly IDeviceService _deviceService;

        
        public WorkOrderEditViewModel(
            IWorkOrderService workOrderService,
            IDeviceService deviceService)
        {
            _workOrderService = workOrderService;
            _deviceService = deviceService;

            PriorityOptions = WorkOrderPriorityOption.AllOptions;
        }

        public bool IsDeviceEditable => !_editingId.HasValue;

        private int? _editingId;

        [ObservableProperty] private string _title = "新增工单";
        [ObservableProperty] private string _orderTitle = string.Empty;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private WorkOrderPriorityOption? _selectedPriority;
        [ObservableProperty] private DeviceDto? _selectedDevice;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string? _errorMessage;

        public IReadOnlyList<WorkOrderPriorityOption> PriorityOptions { get; }
        public ObservableCollection<DeviceDto> DeviceOptions { get; } = new();

        public async Task InitializeAsync(int? orderId)
        {
            _editingId = orderId;

            await LoadDevicesAsync();

            if (orderId.HasValue)
            {
                Title = "编辑工单";
                await LoadOrderAsync(orderId.Value);
            }
            else
            {
                Title = "新增工单";
                SelectedPriority = PriorityOptions.FirstOrDefault(o => o.Value == WorkOrderPriority.Medium);
                SelectedDevice = DeviceOptions.FirstOrDefault();
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsSaving) return;   // 防止重复点击
            if (!Validate()) return;
            IsSaving = true;
            ErrorMessage = null;

            try
            {
                if (_editingId.HasValue)
                {
                    await _workOrderService.UpdateAsync(_editingId.Value, new UpdateWorkOrderRequest
                    {
                        Title = OrderTitle.Trim(),
                        Description = Description.Trim(),
                        Priority = SelectedPriority!.Value
                    });
                }
                else
                {
                    await _workOrderService.CreateAsync(new CreateWorkOrderRequest
                    {
                        DeviceId = SelectedDevice!.Id,
                        Title = OrderTitle.Trim(),
                        Description = Description.Trim(),
                        Priority = SelectedPriority!.Value,
                        CreatedBy = "admin"
                    });
                }

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

        private async Task LoadDevicesAsync()
        {
            try
            {
                var result = await _deviceService.GetPagedAsync(new DeviceQueryDto
                {
                    Page = 1,
                    PageSize = 200
                });

                DeviceOptions.Clear();
                foreach (var d in result.Items) DeviceOptions.Add(d);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载设备失败：{ex.Message}";
            }
        }

        private async Task LoadOrderAsync(int id)
        {
            try
            {
                var order = await _workOrderService.GetByIdAsync(id);
                if (order == null) { ErrorMessage = $"工单 {id} 不存在"; return; }

                OrderTitle = order.Title;
                Description = order.Description;
                SelectedPriority = PriorityOptions.FirstOrDefault(o => o.Value == order.Priority);
                SelectedDevice = DeviceOptions.FirstOrDefault(d => d.Id == order.DeviceId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载工单失败：{ex.Message}";
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(OrderTitle)) { ErrorMessage = "标题不能为空"; return false; }
            if (string.IsNullOrWhiteSpace(Description)) { ErrorMessage = "故障描述不能为空"; return false; }
            if (SelectedPriority == null) { ErrorMessage = "请选择优先级"; return false; }
            if (!_editingId.HasValue && SelectedDevice == null) { ErrorMessage = "请选择设备"; return false; }
            return true;
        }
    }
}
