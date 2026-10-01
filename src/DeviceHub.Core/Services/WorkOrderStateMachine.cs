using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Services
{
    /// <summary>
    /// 工单状态机：控制工单状态流转，禁止非法跳步
    /// 允许：Pending -> Processing -> Closed
    /// </summary>
    public static class WorkOrderStateMachine
    {
        private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> AllowedTransitions = new()
        {
            [WorkOrderStatus.Pending] = new[] { WorkOrderStatus.Processing },
            [WorkOrderStatus.Processing] = new[] { WorkOrderStatus.Closed },
            [WorkOrderStatus.Closed] = Array.Empty<WorkOrderStatus>()
        };

        public static bool CanTransition(WorkOrderStatus from, WorkOrderStatus to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        public static void EnsureCanTransition(WorkOrderStatus from, WorkOrderStatus to)
        {
            if (!CanTransition(from, to))
            {
                throw new BusinessException(
                    $"工单状态不能从「{GetStatusText(from)}」流转到「{GetStatusText(to)}」");
            }
        }

        public static string GetStatusText(WorkOrderStatus status) => status switch
        {
            WorkOrderStatus.Pending => "待处理",
            WorkOrderStatus.Processing => "处理中",
            WorkOrderStatus.Closed => "已关闭",
            _ => "未知"
        };
    }
}
