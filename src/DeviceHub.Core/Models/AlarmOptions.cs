using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Models
{
    /// <summary>
    /// 报警阈值配置
    /// </summary>
    public class AlarmOptions
    {
        public const string SectionName = "Alarm";

        /// <summary>
        /// 温度上限（℃）
        /// </summary>
        public double TemperatureMax { get; set; } = 80;

        /// <summary>
        /// 压力上限（kPa）
        /// </summary>
        public double PressureMax { get; set; } = 150;

        /// <summary>
        /// 转速下限（RPM）
        /// </summary>
        public double SpeedMin { get; set; } = 500;

        /// <summary>
        /// 同一报警去重间隔（秒）
        /// </summary>
        public int SuppressIntervalSeconds { get; set; } = 30;
    }
}
