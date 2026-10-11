using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHub.Client.Attributes;
using DeviceHub.Controls.Controls;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.ViewModels
{
    [ViewModel(ServiceLifetime.Transient)]
    public partial class UserEditViewModel : ObservableObject, IDialogAware
    {
        private readonly IAuthService _authService;
        private int? _editingId;

        public UserEditViewModel(IAuthService authService)
        {
            _authService = authService;
            RoleOptions = new List<UserRoleOption>
        {
            new() { Value = UserRole.Admin, Display = "管理员" },
            new() { Value = UserRole.Operator, Display = "操作员" }
        };
            SelectedRole = RoleOptions.First();
        }

        public event Action<bool>? RequestClose;

        public IReadOnlyList<UserRoleOption> RoleOptions { get; }

        [ObservableProperty] private string _title = "新增用户";
        [ObservableProperty] private string _username = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _displayName = string.Empty;
        [ObservableProperty] private UserRoleOption? _selectedRole;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private bool _isEditMode;

        [ObservableProperty] private bool _canEditUsername;
        [ObservableProperty] private bool _canEditPassword;

        public async Task InitializeAsync(int? userId)
        {
            _editingId = userId;

            if (userId.HasValue)
            {
                Title = "编辑用户";
                IsEditMode = true;
                CanEditUsername = false;   // 编辑时用户名只读
                CanEditPassword = false;   // 编辑时不改密码
                var u = await _authService.GetUserByIdAsync(userId.Value);
                if (u is null)
                {
                    ErrorMessage = "用户不存在";
                    return;
                }
                Username = u.Username;
                DisplayName = u.DisplayName;
                SelectedRole = RoleOptions.FirstOrDefault(r => r.Value == u.Role);
            }
            else
            {
                Title = "新增用户";
                IsEditMode = false;
                CanEditUsername = true;
                CanEditPassword = true;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (IsSaving) return;

            if (string.IsNullOrWhiteSpace(Username)) { ErrorMessage = "用户名不能为空"; return; }
            if (!IsEditMode && (string.IsNullOrWhiteSpace(Password) || Password.Length < 6))
            {
                ErrorMessage = "密码至少 6 位"; return;
            }
            if (string.IsNullOrWhiteSpace(DisplayName)) { ErrorMessage = "显示名不能为空"; return; }
            if (SelectedRole == null) { ErrorMessage = "请选择角色"; return; }

            IsSaving = true;
            ErrorMessage = null;

            try
            {
                if (IsEditMode)
                {
                    if (!_editingId.HasValue)
                    {
                        ErrorMessage = "无效的用户 ID";
                        return;
                    }

                    await _authService.UpdateUserAsync(new UpdateUserRequest
                    {
                        Id = _editingId.Value,
                        DisplayName = DisplayName.Trim(),
                        Role = SelectedRole!.Value,
                        NewPassword = string.IsNullOrWhiteSpace(Password) ? null : Password
                    });
                }
                else
                {
                    await _authService.CreateUserAsync(new CreateUserRequest
                    {
                        Username = Username.Trim(),
                        Password = Password,
                        DisplayName = DisplayName.Trim(),
                        Role = SelectedRole.Value
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
    }

    public class UserRoleOption
    {
        public UserRole Value { get; set; }
        public string Display { get; set; } = string.Empty;
    }
}
