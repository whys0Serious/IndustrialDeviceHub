using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 会话服务：保存"当前登录用户"，全局单例
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        LoginResponse? CurrentUser { get; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// 是否管理员
        /// </summary>
        bool IsAdmin { get; }

        /// <summary>
        /// 设置当前用户
        /// </summary>
        void SetCurrentUser(LoginResponse user);

        /// <summary>
        /// 清除当前用户（登出）
        /// </summary>
        void Clear();
    }
}
