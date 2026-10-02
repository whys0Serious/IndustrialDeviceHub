using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Controls.Controls;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class DeviceCategoryEditViewModel : ObservableObject,IDialogAware
    {
        private readonly IDeviceCategoryService _categoryService;

        public DeviceCategoryEditViewModel(IDeviceCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public event Action<bool>? RequestClose;

        private int? _editingId;

        [ObservableProperty] private string _title = "新增分类";
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string? _errorMessage;

        public async Task InitializeAsync(int? categoryId)
        {
            _editingId = categoryId;

            if (categoryId.HasValue)
            {
                Title = "编辑分类";
                var c = await _categoryService.GetByIdAsync(categoryId.Value);
                if (c == null) { ErrorMessage = "分类不存在"; return; }
                Name = c.Name;
                Description = c.Description;
            }
            else
            {
                Title = "新增分类";
            }
        }

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
                    await _categoryService.UpdateAsync(_editingId.Value, new UpdateDeviceCategoryDto
                    {
                        Name = Name.Trim(),
                        Description = Description?.Trim()
                    });
                }
                else
                {
                    await _categoryService.CreateAsync(new CreateDeviceCategoryDto
                    {
                        Name = Name.Trim(),
                        Description = Description?.Trim()
                    });
                }
                RequestClose?.Invoke(true);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "分类名称不能为空"; return false; }
            return true;
        }
    }
}
