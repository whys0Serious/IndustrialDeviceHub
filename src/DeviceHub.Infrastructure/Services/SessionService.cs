using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 会话服务：保存当前登录用户（单例）
    /// </summary>
    public class SessionService : ISessionService
    {
        private LoginResponse? _currentUser;

        public LoginResponse? CurrentUser => _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

        public void SetCurrentUser(LoginResponse user) => _currentUser = user;

        public void Clear() => _currentUser = null;
    }
}
