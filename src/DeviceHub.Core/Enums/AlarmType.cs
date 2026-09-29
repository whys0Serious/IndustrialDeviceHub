using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Enums
{
    /// <summary>
    /// 报警类型
    /// </summary>
    public enum AlarmType
    {
        /// <summary>
        /// 温度过高
        /// </summary>
        TemperatureHigh = 1,

        /// <summary>
        /// 压力过高
        /// </summary>
        PressureHigh = 2,

        /// <summary>
        /// 转速过低
        /// </summary>
        SpeedLow = 3,

        /// <summary>
        /// 连接断开
        /// </summary>
        ConnectionLost = 4
    }
}
