using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class DeviceCategoryListViewModel : ObservableObject
    {
        private readonly IDeviceCategoryService _categoryService;
        private readonly IDialogService _dialogService;

        public DeviceCategoryListViewModel(
            IDeviceCategoryService categoryService,
            IDialogService dialogService)
        {
            _categoryService = categoryService;
            _dialogService = dialogService;
        }

        [ObservableProperty]
        private DeviceCategoryDto? _selectedCategory;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        public ObservableCollection<DeviceCategoryDto> Categories { get; } = new();

        public bool HasData => Categories.Count > 0;

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                var list = await _categoryService.GetAllAsync();
                Categories.Clear();
                foreach (var c in list) Categories.Add(c);
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

        [RelayCommand]
        private async Task CreateAsync()
        {
              var result = _dialogService.ShowDialog<DeviceCategoryEditViewModel, DeviceCategoryEditView>(
                vm => _ = vm.InitializeAsync(null));
            if (result==true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("分类创建成功");
            }
            
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedCategory == null) return;
            var id = SelectedCategory.Id;
            var result = _dialogService.ShowDialog<DeviceCategoryEditViewModel, DeviceCategoryEditView>(
                vm => _ = vm.InitializeAsync(id));
            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("分类编辑成功");
            }
            
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;

            //先保存分类名，因为LoadAsync后SelectedCategory会变 null
            var categoryName = SelectedCategory.Name;

            var confirmed = _dialogService.Confirm(
                $"确定要删除分类「{categoryName}」吗？\n\n如果该分类下有设备，将无法删除。",
                "删除确认");

            if (!confirmed) return;

            try
            {
                await _categoryService.DeleteAsync(SelectedCategory.Id);
                await LoadAsync();

                // 用保存的 categoryName，不用 SelectedCategory.Name
                _dialogService.ShowSuccess($"分类「{categoryName}」已删除");
            }
            catch (Exception ex)
            {
                _dialogService.ShowWarning($"无法删除：{ex.Message}");
            }
        }

        private bool CanEdit() => SelectedCategory != null;

        partial void OnSelectedCategoryChanged(DeviceCategoryDto? value)
        {
            EditCommand.NotifyCanExecuteChanged();
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }
}
