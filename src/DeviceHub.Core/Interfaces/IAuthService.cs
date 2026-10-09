using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// 登录
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        /// <summary
        /// >创建用户（管理员）
        /// </summary>
        Task<int> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);

        /// <summary>
        /// 获取所有用户
        /// </summary>
        Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default);

        /// <summary>
        /// 启用/禁用用户
        /// </summary>
        Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default);

        /// <summary>
        /// 修改密码
        /// </summary>
        Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default);

        /// <summary>
        /// 初始化默认管理员（首次启动时调用）
        /// </summary>
        Task EnsureDefaultAdminAsync(CancellationToken ct = default);
    }
}
