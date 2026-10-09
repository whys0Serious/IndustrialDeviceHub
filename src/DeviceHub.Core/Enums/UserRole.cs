using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Enums
{
    public enum UserRole
    {
        /// <summary>
        /// 管理员（全部权限）
        /// </summary>
        Admin = 1,

        /// <summary>
        /// 操作员（设备查看、工单创建、AI 助手）
        /// </summary>
        Operator = 2
    }
}
