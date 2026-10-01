using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Client.Models
{
    /// <summary>
    /// 工单属性列表选项
    /// </summary>
    public class WorkOrderPriorityOption
    {
        public WorkOrderPriority Value { get; init; }
        public string Display { get; init; } = string.Empty;

        public static WorkOrderPriorityOption All { get; } = new()
        {
            Value = (WorkOrderPriority)(-1),
            Display = "全部"
        };

        public static IReadOnlyList<WorkOrderPriorityOption> AllOptions { get; } = new List<WorkOrderPriorityOption>
    {
        new() { Value = WorkOrderPriority.Low,    Display = "低" },
        new() { Value = WorkOrderPriority.Medium, Display = "中" },
        new() { Value = WorkOrderPriority.High,   Display = "高" },
        new() { Value = WorkOrderPriority.Urgent, Display = "紧急" }
    };

        public static IReadOnlyList<WorkOrderPriorityOption> FilterOptions { get; } = new List<WorkOrderPriorityOption>
    {
        All,
        new() { Value = WorkOrderPriority.Low,    Display = "低" },
        new() { Value = WorkOrderPriority.Medium, Display = "中" },
        new() { Value = WorkOrderPriority.High,   Display = "高" },
        new() { Value = WorkOrderPriority.Urgent, Display = "紧急" }
    };
    }
}
