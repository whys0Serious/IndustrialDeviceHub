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

        public ObservableCollection<Recipe> Recipes { get; } = new();
        [ObservableProperty] private Recipe? _selectedRecipe;

        public RecipeListViewModel(IRecipeService recipeService, IDialogService dialogService)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
        }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            var list = await _recipeService.GetAllAsync();
            Recipes.Clear();
            foreach (var r in list) Recipes.Add(r);
        }

        [RelayCommand]
        private async Task AddRecipeAsync()
        {
            var result = _dialogService.ShowDialog<RecipeEditViewModel, RecipeEditView>(vm => vm.Initialize());
            if (result == true) await LoadAsync();
        }

        [RelayCommand]
        private async Task EditRecipeAsync()
        {
            if (SelectedRecipe == null) return;

            var result = _dialogService.ShowDialog<RecipeEditViewModel, RecipeEditView>(
                vm => vm.Initialize(SelectedRecipe));

            if (result == true) await LoadAsync();
        }

        [RelayCommand]
        private async Task DeleteRecipeAsync()
        {
            if (SelectedRecipe == null) return;
            if (!_dialogService.Confirm($"确定删除配方「{SelectedRecipe.Name}」吗？", "删除确认")) return;

            await _recipeService.DeleteAsync(SelectedRecipe.Id);
            await LoadAsync();
        }
    }
}
