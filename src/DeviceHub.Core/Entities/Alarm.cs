using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Entities
{
    /// <summary>
    /// 报警记录
    /// </summary>
    public class Alarm
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 关联设备
        /// </summary>
        public Device? Device { get; set; }

        /// <summary>
        /// 报警类型
        /// </summary>
        public AlarmType Type { get; set; }

        /// <summary>
        /// 报警消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 触发时的值
        /// </summary>
        public double TriggerValue { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        public double ThresholdValue { get; set; }

        /// <summary>
        /// 报警状态
        /// </summary>
        public AlarmStatus Status { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 确认时间
        /// </summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// 确认人
        /// </summary>
        public string? AcknowledgedBy { get; set; }

        /// <summary>
        /// 恢复时间（参数回到正常范围时记录）
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }
}
