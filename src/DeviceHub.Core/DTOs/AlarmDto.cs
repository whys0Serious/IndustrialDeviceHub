using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 报警记录DTO
    /// </summary>
    public class AlarmDto
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }

        /// <summary>
        /// 显示用的设备名（处理 null）
        /// </summary>
        public string DeviceNameDisplay => string.IsNullOrEmpty(DeviceName) ? "（设备已删除）" : DeviceName;
        public AlarmType Type { get; set; }
        public string TypeText => Type switch
        {
            AlarmType.TemperatureHigh => "温度过高",
            AlarmType.PressureHigh => "压力过高",
            AlarmType.SpeedLow => "转速过低",
            AlarmType.ConnectionLost => "连接断开",
            _ => "未知"
        };
        public string Message { get; set; } = string.Empty;
        public double TriggerValue { get; set; }
        public double ThresholdValue { get; set; }
        public AlarmStatus Status { get; set; }
        public string StatusText => Status == AlarmStatus.Unacknowledged ? "未确认" : "已确认";
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// 是否已确认（XAML绑定颜色）
        /// </summary>
        public bool IsAcknowledged => Status == AlarmStatus.Acknowledged;
    }
}
