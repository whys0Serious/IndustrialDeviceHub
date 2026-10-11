using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Client.Services;
using DeviceHub.Client.Views;
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
    public partial class UserListViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;

        public UserListViewModel(IAuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;
        }

        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private UserDto? _selectedUser;

        public ObservableCollection<UserDto> Users { get; } = new();

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
                var list = await _authService.GetAllUsersAsync();
                Users.Clear();
                foreach (var u in list) Users.Add(u);
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
            var result = _dialogService.ShowDialog<UserEditViewModel, UserEditView>(
                vm => vm.InitializeAsync(null));

            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("用户创建成功");
            }
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task EditAsync()
        {
            if (SelectedUser == null) return;

            var id = SelectedUser.Id;
            var result = _dialogService.ShowDialog<UserEditViewModel, UserEditView>(
                vm => vm.InitializeAsync(id));

            if (result == true)
            {
                await LoadAsync();
                _dialogService.ShowSuccess("用户已更新");
            }
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task ToggleActiveAsync()
        {
            if (SelectedUser == null) return;

            var target = !SelectedUser.IsActive;
            var action = target ? "启用" : "禁用";

            if (!_dialogService.Confirm($"确定要{action}用户「{SelectedUser.Username}」吗？", $"{action}用户"))
                return;

            try
            {
                await _authService.SetActiveAsync(SelectedUser.Id, target);
                await LoadAsync();
                _dialogService.ShowSuccess($"用户已{action}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"操作失败：{ex.Message}");
            }
        }

        private bool CanEdit() => SelectedUser != null;

        partial void OnSelectedUserChanged(UserDto? value)
        {
            EditCommand.NotifyCanExecuteChanged();
            ToggleActiveCommand.NotifyCanExecuteChanged();
        }
    }
}
