using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 设备状态统计
    /// </summary>
    public class DeviceStatisticsDto
    {
        /// <summary>
        /// 设备总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 运行中数量
        /// </summary>
        public int RunningCount { get; set; }

        /// <summary>
        /// 报警数量
        /// </summary>
        public int AlarmCount { get; set; }

        /// <summary>
        /// 停止数量
        /// </summary>
        public int StoppedCount { get; set; }
    }
}
