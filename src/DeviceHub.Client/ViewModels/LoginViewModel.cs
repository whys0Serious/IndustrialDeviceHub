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
    public partial class LoginViewModel : ObservableObject, IDialogAware
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _session;

        public LoginViewModel(IAuthService authService, ISessionService session)
        {
            _authService = authService;
            _session = session;
        }

        public event Action<bool>? RequestClose;

        [ObservableProperty] private string _username = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string? _errorMessage;
        [ObservableProperty] private bool _isLoading;

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "请输入用户名和密码";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var result = await _authService.LoginAsync(new LoginRequest
                {
                    Username = Username.Trim(),
                    Password = Password
                });

                _session.SetCurrentUser(result);
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }
    }
}
