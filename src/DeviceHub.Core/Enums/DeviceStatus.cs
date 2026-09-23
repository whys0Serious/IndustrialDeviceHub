using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Enums
{
    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// 停止
        /// </summary>
        Stopped = 0,
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,
        /// <summary>
        /// 警报
        /// </summary>
        Alarm = 2
    }
}
