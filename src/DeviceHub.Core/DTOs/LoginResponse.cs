using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        /// <summary>
        /// 角色显示文本
        /// </summary>
        public string RoleText => Role == UserRole.Admin ? "管理员" : "操作员";
    }
}
