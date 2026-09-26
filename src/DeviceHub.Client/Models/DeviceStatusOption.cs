using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Models
{
    /// <summary>
    /// 状态下拉框选项
    /// </summary>
    public class DeviceStatusOption
    {
        public DeviceStatus Value { get; init; }
        public string Display { get; init; } = string.Empty;

        /// <summary>
        /// 全部选项（哨兵值-1，ViewModel判断时当null处理）
        /// </summary>
        public static DeviceStatusOption All { get; } = new()
        {
            Value = (DeviceStatus)(-1),
            Display = "全部"
        };

        public static IReadOnlyList<DeviceStatusOption> AllOptions { get; } = new List<DeviceStatusOption>
    {
        All,
        new() { Value = DeviceStatus.Stopped, Display = "已停止"},
        new() { Value = DeviceStatus.Running, Display = "运行中"},
        new() { Value = DeviceStatus.Alarm,   Display = "报警"}
    };
    }
}
