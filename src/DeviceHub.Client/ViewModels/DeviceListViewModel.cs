using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Models;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class DeviceListViewModel : ObservableObject
    {
        private readonly IDeviceService _deviceService;
        private readonly IDeviceCategoryService _categoryService;

        private readonly IDialogService _dialogService;

        private bool _isInitializing = true;//保护状态
        /// <summary>
        /// 统计设备状态
        /// </summary>
        [ObservableProperty]
        private int _totalDevices;

        [ObservableProperty]
        private int _runningCount;

        [ObservableProperty]
        private int _alarmCount;

        [ObservableProperty]
        private int _stoppedCount;

        public DeviceListViewModel(IDeviceService deviceService, IDeviceCategoryService categoryService, IDialogService dialogService)
        {
            _deviceService = deviceService;
            _categoryService = categoryService;
            _dialogService = dialogService;

            StatusOptions = DeviceStatusOption.AllOptions;
            SelectedStatus = DeviceStatusOption.All;
        }

        /// <summary>
        /// 搜索/筛选
        /// </summary>

        [ObservableProperty]
        private string? _searchKeyword;

        [ObservableProperty]
        private DeviceStatusOption? _selectedStatus;

        [ObservableProperty]
        private DeviceCategoryDto? _selectedCategory;

        public IReadOnlyList<DeviceStatusOption> StatusOptions { get; }

        public ObservableCollection<DeviceCategoryDto> CategoryOptions { get; } = new();

        /// <summary>
        /// 分页
        /// </summary>

        [ObservableProperty]
        private int _currentPageIndex = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private int _totalCount;

        /// <summary>
        /// 状态
        /// </summary>

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        /// <summary>
        /// 当前选中的设备
        /// </summary>
        [ObservableProperty]
        private DeviceDto? _selectedDevice;
        /// <summary>
        /// 编辑条件判断
        /// </summary>
        /// <returns></returns>
        private bool CanEdit() => SelectedDevice != null;

        public bool HasData => Devices.Count > 0;

        /// <summary>
        /// 数据
        /// </summary>

        public ObservableCollection<DeviceDto> Devices { get; } = new();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>

        public async Task InitializeAsync()
        {
            _isInitializing = true;   // 开始初始化
            await LoadCategoriesAsync();
            await LoadStatisticsAsync();
            await LoadPageAsync();
            _isInitializing = false;  // 初始化完成
        }
       
        /// <summary>
        /// 搜索（重置到第 1 页）
        /// </summary>
        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPageIndex = 1;
            //await LoadStatisticsAsync();//数据变化了 统计要刷新
            await LoadPageAsync();
        }

        /// <summary>
        /// 重置筛选条件
        /// </summary>
        [RelayCommand]
        private async Task ResetAsync()
        {
            SearchKeyword = null;
            SelectedStatus = DeviceStatusOption.All;
            SelectedCategory = CategoryOptions.FirstOrDefault();
            CurrentPageIndex = 1;
            await LoadStatisticsAsync();
            await LoadPageAsync();
        }



        /// <summary>
        /// Pagination翻页后触发
        /// </summary>
        [RelayCommand]
        private async Task PageUpdatedAsync()
        {
            await LoadPageAsync(); //翻页不需要刷新统计
        }

        /// <summary>
        /// 新增设备
        /// </summary>
        [RelayCommand]
        private async Task CreateAsync()
        {
            var result = _dialogService.ShowDialog<DeviceEditViewModel, DeviceEditView>(
                vm => _ = vm.InitializeAsync(null));

            if (result == true)
            {
                await LoadStatisticsAsync();
                await LoadPageAsync();
                _dialogService.ShowSuccess("设备创建成功");
            }
        }

        /// <summary>
        /// 编辑选中的设备
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedDevice == null) return;

            var id = SelectedDevice.Id;
            var result = _dialogService.ShowDialog<DeviceEditViewModel, DeviceEditView>(
                vm => _ = vm.InitializeAsync(id));
            if (result == true)
            {
                await LoadStatisticsAsync();
                await LoadPageAsync();
                _dialogService.ShowSuccess("设备编辑成功");
            }
            
        }

        /// <summary>
        /// 删除选中设备
        /// </summary>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task DeleteAsync()
        {
            if (SelectedDevice == null) return;

            // 先保存名字和Id
            var deviceName = SelectedDevice.Name;
            var deviceCode = SelectedDevice.Code;
            var deviceId = SelectedDevice.Id;

            var confirmed = _dialogService.Confirm(
                $"确定要删除设备「{deviceName}」（{deviceCode}）吗？\n\n删除后可在数据库中恢复。",
                "删除确认");

            if (!confirmed) return;

            try
            {
                await _deviceService.DeleteAsync(deviceId);
                await LoadStatisticsAsync();
                await LoadPageAsync();

                _dialogService.ShowSuccess($"设备「{deviceName}」已删除");
            }
            catch (InvalidOperationException ex)
            {
                _dialogService.ShowWarning(ex.Message, "无法删除");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"删除失败：{ex.Message}", "错误");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();

                CategoryOptions.Clear();
                CategoryOptions.Add(new DeviceCategoryDto { Id = -1, Name = "全部分类" });
                foreach (var c in categories) CategoryOptions.Add(c);

                SelectedCategory = CategoryOptions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载分类失败：{ex.Message}";
            }
        }

        private async Task LoadPageAsync()
        {
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var query = new DeviceQueryDto
                {
                    Page = CurrentPageIndex,
                    PageSize = PageSize,
                    Keyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword.Trim(),
                    Status = SelectedStatus?.Value == (Core.Enums.DeviceStatus)(-1) ? null : SelectedStatus?.Value,
                    CategoryId = SelectedCategory?.Id == -1 ? null : SelectedCategory?.Id
                };

                var result = await _deviceService.GetPagedAsync(query);

                Devices.Clear();
                foreach (var d in result.Items) Devices.Add(d);

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPageIndex = result.Page;   // 双向同步（Pagination 需要）

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

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var stats = await _deviceService.GetStatisticsAsync();
                TotalDevices = stats.TotalCount;
                RunningCount = stats.RunningCount;
                AlarmCount = stats.AlarmCount;
                StoppedCount = stats.StoppedCount;
            }
            catch (Exception ex)
            {
                //统计失败不影响主流程
                System.Diagnostics.Debug.WriteLine($"统计加载失败：{ex.Message}");
            }
        }
        /// <summary>
        /// 通知命令
        /// </summary>
        /// <param name="value"></param>
        partial void OnSelectedDeviceChanged(DeviceDto? value)
        {
            EditCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// 状态变化时自动搜索
        /// </summary>
        partial void OnSelectedStatusChanged(DeviceStatusOption? value)
        {
            //防止初始化时触发（CategoryOptions 还没加载）
            if (_isInitializing) return;
            _ = SearchAsync();;
        }

        /// <summary>
        /// 分类变化时自动搜索
        /// </summary>
        partial void OnSelectedCategoryChanged(DeviceCategoryDto? value)
        {
            if (_isInitializing) return;
            _ = SearchAsync();
        }
    }

}
