using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 单台设备的实时数据
    /// </summary>
    public class RealtimeDataDto
    {
        /// <summary>
        /// 设备 Id
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 温度（℃）
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// 压力（kPa）
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// 转速（RPM）
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
