using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Enums
{
    /// <summary>
    /// 工单状态
    /// </summary>
    public enum WorkOrderStatus
    {
        /// <summary>
        /// 待处理
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 处理中
        /// </summary>
        Processing = 1,

        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 2
    }
}
