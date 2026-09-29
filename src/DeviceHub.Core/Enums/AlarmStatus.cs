using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Enums
{
    /// <summary>
    /// 报警状态
    /// </summary>
    public enum AlarmStatus
    {
        /// <summary>
        /// 未确认
        /// </summary>
        Unacknowledged = 0,

        /// <summary>
        /// 已确认
        /// </summary>
        Acknowledged = 1
    }
}
