using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class RecipeListViewModel : ObservableObject
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        // 防抖锁
        private readonly SemaphoreSlim _loadLock = new(1, 1);
        public ObservableCollection<Recipe> Recipes { get; } = new();

        // 搜索与分页
        [ObservableProperty] private string? _searchKeyword;
        [ObservableProperty] private int _currentPageIndex = 1;
        [ObservableProperty] private int _pageSize = 10;
        [ObservableProperty] private int _totalPages;
        [ObservableProperty] private int _totalCount;

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string? _errorMessage;

        // 选中状态与命令联动
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditRecipeCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteRecipeCommand))]
        private Recipe? _selectedRecipe;

        private bool CanModify() => SelectedRecipe != null;
        public RecipeListViewModel(IRecipeService recipeService, IDialogService dialogService)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
        }

        public async Task InitializeAsync()
        {
            await LoadPageAsync();
        }
        /// <summary>
        /// 搜索（重置到第一页）
        /// </summary>
        [RelayCommand]
        private async Task SearchAsync()
        {
            if (IsLoading) return;
            CurrentPageIndex = 1;
            await LoadPageAsync();
        }

        /// <summary>
        /// Pagination 翻页触发
        /// </summary>
        [RelayCommand]
        private async Task PageUpdatedAsync()
        {
            await LoadPageAsync();
        }


        /// <summary>
        /// 加载当前页数据
        /// </summary>
        private async Task LoadPageAsync()
        {
            if (!await _loadLock.WaitAsync(0)) return;

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var result = await _recipeService.GetPagedAsync(SearchKeyword, CurrentPageIndex, PageSize);

                Recipes.Clear();
                foreach (var r in result.Items) Recipes.Add(r);

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPageIndex = result.Page;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载配方失败：{ex.Message}";
            }
            finally
            {
                IsLoading = false;
                _loadLock.Release();
            }
        }


        [RelayCommand]
        private async Task AddRecipeAsync()
        {
            var result = _dialogService.ShowDialog<RecipeEditViewModel, RecipeEditView>(vm => vm.Initialize());
            if (result == true) await LoadPageAsync();
        }

        [RelayCommand]
        private async Task EditRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            var result = _dialogService.ShowDialog<RecipeEditViewModel, RecipeEditView>(
                vm => vm.Initialize(SelectedRecipe));

            if (result == true) await LoadPageAsync();
        }

        [RelayCommand]
        private async Task DeleteRecipeAsync()
        {
            if (SelectedRecipe == null) return;
            if (!_dialogService.Confirm($"确定删除配方「{SelectedRecipe.Name}」吗？", "删除确认")) return;

            try
            {
                await _recipeService.DeleteAsync(SelectedRecipe.Id);
                await LoadPageAsync();
                _dialogService.ShowSuccess("配方已删除");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"删除失败：{ex.Message}");
            }
        }

        // 搜索框内容变化时，自动触发搜索（可选：也可以在界面上加个按钮）
        partial void OnSearchKeywordChanged(string? value)
        {
            _ = SearchAsync();
        }
    }
}
