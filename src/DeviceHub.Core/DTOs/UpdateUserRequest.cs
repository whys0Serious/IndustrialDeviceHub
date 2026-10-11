using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class UpdateUserRequest
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        /// <summary>
        /// 可选：管理员重置密码。为空则不修改密码。
        /// </summary>
        public string? NewPassword { get; set; }
    }
}
