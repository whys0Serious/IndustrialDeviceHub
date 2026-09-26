using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Models;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class DeviceEditViewModel : ObservableObject
    {
        private readonly IDeviceService _deviceService;
        private readonly IDeviceCategoryService _categoryService;

        public DeviceEditViewModel(IDeviceService deviceService, IDeviceCategoryService categoryService)
        {
            _deviceService = deviceService;
            _categoryService = categoryService;

            StatusOptions = DeviceStatusOption.AllOptions
                .Where(o => o.Value != (Core.Enums.DeviceStatus)(-1))
                .ToList();
        }

        //---------- 请求关闭事件----------

        /// <summary>
        /// 请求关闭对话框
        /// true = 保存，false = 取消
        /// </summary>
        public event Action<bool>? RequestClose;

        /// <summary>
        /// 新增或修改
        /// </summary>

        private int? _editingId;

        [ObservableProperty]
        private string _title = "新增设备";

        // ---------- 表单字段 ----------

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private DeviceStatusOption? _selectedStatus;

        [ObservableProperty]
        private DeviceCategoryDto? _selectedCategory;

        [ObservableProperty]
        private string? _remark;

        public IReadOnlyList<DeviceStatusOption> StatusOptions { get; }

        public ObservableCollection<DeviceCategoryDto> CategoryOptions { get; } = new();

        // ---------- 状态 ----------

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string? _errorMessage;

        // ---------- 初始化 ----------

        public async Task InitializeAsync(int? deviceId)
        {
            _editingId = deviceId;

            await LoadCategoriesAsync();

            if (deviceId.HasValue)
            {
                Title = "编辑设备";
                await LoadDeviceAsync(deviceId.Value);
            }
            else
            {
                Title = "新增设备";
                SelectedStatus = StatusOptions.FirstOrDefault();
                SelectedCategory = CategoryOptions.FirstOrDefault();
            }
        }

        // ---------- 命令 ----------

        /// <summary>
        /// 保存设备信息
        /// </summary>
        /// <returns></returns>

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                if (_editingId.HasValue)
                {
                    var request = new UpdateDeviceDto
                    {
                        Name = Name.Trim(),
                        Code = Code.Trim(),
                        Status = SelectedStatus!.Value,
                        CategoryId = SelectedCategory!.Id,
                        Remark = Remark?.Trim()
                    };
                    await _deviceService.UpdateAsync(_editingId.Value, request);
                }
                else
                {
                    var request = new CreateDeviceDto
                    {
                        Name = Name.Trim(),
                        Code = Code.Trim(),
                        Status = SelectedStatus!.Value,
                        CategoryId = SelectedCategory!.Id,
                        Remark = Remark?.Trim()
                    };
                    await _deviceService.CreateAsync(request);
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
        /// <summary>
        /// 关闭窗口行为
        /// </summary>

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }

        // ---------- 内部方法 ----------
        /// <summary>
        /// 加载全部分类
        /// </summary>
        /// <returns></returns>
        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                CategoryOptions.Clear();
                foreach (var c in categories) CategoryOptions.Add(c);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载分类失败：{ex.Message}";
            }
        }
        /// <summary>
        /// ID加载某个设备信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task LoadDeviceAsync(int id)
        {
            try
            {
                var device = await _deviceService.GetByIdAsync(id);
                if (device == null)
                {
                    ErrorMessage = $"设备 {id} 不存在";
                    return;
                }

                Name = device.Name;
                Code = device.Code;
                SelectedStatus = StatusOptions.FirstOrDefault(o => o.Value == device.Status);
                SelectedCategory = CategoryOptions.FirstOrDefault(c => c.Id == device.CategoryId);
                Remark = device.Remark;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载设备失败：{ex.Message}";
            }
        }
        /// <summary>
        /// 校验
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "设备名称不能为空"; return false; }
            if (string.IsNullOrWhiteSpace(Code)) { ErrorMessage = "设备编号不能为空"; return false; }
            if (SelectedStatus == null) { ErrorMessage = "请选择设备状态"; return false; }
            if (SelectedCategory == null) { ErrorMessage = "请选择设备分类"; return false; }
            return true;
        }
    }
}
