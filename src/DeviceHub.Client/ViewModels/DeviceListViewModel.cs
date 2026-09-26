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
            await LoadCategoriesAsync();
            await LoadStatisticsAsync();
            await LoadPageAsync();
        }


        //-----界面命令-----
       
        /// <summary>
        /// 搜索（重置到第 1 页）
        /// </summary>
        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPageIndex = 1;
            await LoadStatisticsAsync();//数据变化了 统计要刷新
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
            _dialogService.ShowDialog<DeviceEditViewModel, DeviceEditView>(
                vm => _ = vm.InitializeAsync(null));

            // 不管取消还是保存，都刷新
            await LoadStatisticsAsync();
            await LoadPageAsync();
        }

        /// <summary>编辑选中的设备</summary>
        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedDevice == null) return;

            var id = SelectedDevice.Id;
            _dialogService.ShowDialog<DeviceEditViewModel, DeviceEditView>(
                vm => _ = vm.InitializeAsync(id));

            await LoadStatisticsAsync();
            await LoadPageAsync();
        }


        //-------内部方法------

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

        partial void OnSelectedDeviceChanged(DeviceDto? value)
        {
            EditCommand.NotifyCanExecuteChanged();
        }
    }

}
