using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Models
{
    /// <summary>
    /// 工单状态选项
    /// </summary>
    public class WorkOrderStatusOption
    {
        public WorkOrderStatus Value { get; init; }
        public string Display { get; init; } = string.Empty;

        public static WorkOrderStatusOption All { get; } = new()
        {
            Value = (WorkOrderStatus)(-1),
            Display = "全部"
        };

        public static IReadOnlyList<WorkOrderStatusOption> FilterOptions { get; } = new List<WorkOrderStatusOption>
    {
        All,
        new() { Value = WorkOrderStatus.Pending,    Display = "待处理" },
        new() { Value = WorkOrderStatus.Processing, Display = "处理中" },
        new() { Value = WorkOrderStatus.Closed,     Display = "已关闭" }
    };
    }
}
