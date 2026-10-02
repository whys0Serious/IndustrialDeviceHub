using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Models;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
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
    public partial class WorkOrderListViewModel : ObservableObject
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;

        private bool _isInitializing = true;

        public WorkOrderListViewModel(
            IWorkOrderService workOrderService,
            IDeviceService deviceService,
            IDialogService dialogService)
        {
            _workOrderService = workOrderService;
            _deviceService = deviceService;
            _dialogService = dialogService;

            StatusOptions = WorkOrderStatusOption.FilterOptions;
            PriorityOptions = WorkOrderPriorityOption.FilterOptions;

            SelectedStatus = StatusOptions.First();
            SelectedPriority = PriorityOptions.First();
        }

        /// <summary>
        /// 筛选 
        /// </summary>

        [ObservableProperty] private string? _searchKeyword;
        [ObservableProperty] private WorkOrderStatusOption? _selectedStatus;
        [ObservableProperty] private WorkOrderPriorityOption? _selectedPriority;
        [ObservableProperty] private DeviceDto? _selectedDevice;

        public IReadOnlyList<WorkOrderStatusOption> StatusOptions { get; }
        public IReadOnlyList<WorkOrderPriorityOption> PriorityOptions { get; }
        public ObservableCollection<DeviceDto> DeviceOptions { get; } = new();

        /// <summary>
        /// 分页
        /// </summary>

        [ObservableProperty] private int _currentPageIndex = 1;
        [ObservableProperty] private int _pageSize = 20;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private int _totalCount;

        /// <summary>
        /// 状态
        /// </summary>

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string? _errorMessage;

        public bool HasData => WorkOrders.Count > 0;

        /// <summary>
        /// 数据
        /// </summary>

        public ObservableCollection<WorkOrderDto> WorkOrders { get; } = new();

        [ObservableProperty]
        private WorkOrderDto? _selectedWorkOrder;

        private bool _isUpdatingFilters;

        /// <summary>
        /// 初始化
        /// </summary>

        public async Task InitializeAsync()
        {
            _isInitializing = true;

            await LoadDevicesAsync();
            await LoadAsync();

            _isInitializing = false;
        }

        /// <summary>
        /// 筛选变化触发
        /// </summary>
        /// <param name="value"></param>

        partial void OnSelectedStatusChanged(WorkOrderStatusOption? value)
        {
            if (_isInitializing || _isUpdatingFilters) return;
            CurrentPageIndex = 1;
            _ = LoadAsync();
        }

        partial void OnSelectedPriorityChanged(WorkOrderPriorityOption? value)
        {
            if (_isInitializing || _isUpdatingFilters) return;
            CurrentPageIndex = 1;
            _ = LoadAsync();
        }

        partial void OnSelectedDeviceChanged(DeviceDto? value)
        {
            if (_isInitializing || _isUpdatingFilters) return;
            CurrentPageIndex = 1;
            _ = LoadAsync();
        }

        partial void OnSelectedWorkOrderChanged(WorkOrderDto? value)
        {
            StartCommand.NotifyCanExecuteChanged();
            CloseCommand.NotifyCanExecuteChanged();
            EditCommand.NotifyCanExecuteChanged();
            ViewDetailCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// 命令
        /// </summary>

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPageIndex = 1;
            await LoadAsync();
        }

        [RelayCommand]
        private async Task ResetAsync()
        {
            _isUpdatingFilters = true;   //抑制

            try
            {
                SearchKeyword = null;
                SelectedStatus = StatusOptions.First();
                SelectedPriority = PriorityOptions.First();
                SelectedDevice = DeviceOptions.FirstOrDefault();
                CurrentPageIndex = 1;
            }
            finally
            {
                _isUpdatingFilters = false;   //恢复
            }

            await LoadAsync();  
        }

        [RelayCommand]
        private async Task PageUpdatedAsync()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private async Task CreateAsync()
        {
            var result = _dialogService.ShowDialog<WorkOrderEditViewModel, WorkOrderEditView>(
                vm => _ = vm.InitializeAsync(null));

            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("工单创建成功");
            }
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedWorkOrder == null) return;

            var id = SelectedWorkOrder.Id;
            var result = _dialogService.ShowDialog<WorkOrderEditViewModel, WorkOrderEditView>(
                vm => _ = vm.InitializeAsync(id));

            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("工单已更新");
            }
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private async Task StartAsync()
        {
            if (SelectedWorkOrder == null) return;

            var confirmed = _dialogService.Confirm(
                $"确认开始处理工单「{SelectedWorkOrder.Title}」？",
                "开始处理");

            if (!confirmed) return;

            try
            {
                await _workOrderService.StartAsync(SelectedWorkOrder.Id, "admin");
                await LoadAsync();
                _dialogService.ShowSuccess("工单已开始处理");
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning(ex.Message, "无法开始处理");
            }
        }

        [RelayCommand(CanExecute = nameof(CanClose))]
        private async Task CloseAsync()
        {
            if (SelectedWorkOrder == null) return;

            //用对话框输入处理结果
            var result = _dialogService.ShowDialog<WorkOrderCloseViewModel, WorkOrderCloseView>(
                vm => vm.Initialize(SelectedWorkOrder.Id, SelectedWorkOrder.Title));

            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("工单已关闭");
            }
        }

        [RelayCommand(CanExecute = nameof(CanViewDetail))]
        private async Task ViewDetailAsync()
        {
            if (SelectedWorkOrder == null) return;

            var id = SelectedWorkOrder.Id;
            _dialogService.ShowDialog<WorkOrderDetailViewModel, WorkOrderDetailView>(
                vm => _ = vm.InitializeAsync(id));
            await LoadAsync();
        }
        private bool CanViewDetail() => SelectedWorkOrder != null;
        private bool CanEdit() => SelectedWorkOrder != null && SelectedWorkOrder.Status != WorkOrderStatus.Closed;
        private bool CanStart() => SelectedWorkOrder?.Status == WorkOrderStatus.Pending;
        private bool CanClose() => SelectedWorkOrder?.Status == WorkOrderStatus.Processing;

       
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
                DeviceOptions.Add(new DeviceDto { Id = -1, Name = "全部设备" });
                foreach (var d in result.Items) DeviceOptions.Add(d);

                SelectedDevice = DeviceOptions.First();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载设备失败：{ex.Message}";
            }
        }

        private async Task LoadAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var query = new WorkOrderQueryParams
                {
                    PageIndex = CurrentPageIndex,
                    PageSize = PageSize,
                    Keyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                    Status = SelectedStatus?.Value == (WorkOrderStatus)(-1) ? null : SelectedStatus?.Value,
                    Priority = SelectedPriority?.Value == (WorkOrderPriority)(-1) ? null : SelectedPriority?.Value,
                    DeviceId = SelectedDevice?.Id == -1 ? null : SelectedDevice?.Id
                };

                var result = await _workOrderService.GetPagedAsync(query);

                WorkOrders.Clear();
                foreach (var o in result.Items) WorkOrders.Add(o);

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;

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
}
