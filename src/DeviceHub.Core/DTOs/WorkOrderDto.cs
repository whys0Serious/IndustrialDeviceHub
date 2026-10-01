using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class WorkOrderDto
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string DeviceNameDisplay => string.IsNullOrEmpty(DeviceName) ? "（设备已删除）" : DeviceName;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public WorkOrderPriority Priority { get; set; }
        public string PriorityText => Priority switch
        {
            WorkOrderPriority.Low => "低",
            WorkOrderPriority.Medium => "中",
            WorkOrderPriority.High => "高",
            WorkOrderPriority.Urgent => "紧急",
            _ => "未知"
        };

        public WorkOrderStatus Status { get; set; }
        public string StatusText => Status switch
        {
            WorkOrderStatus.Pending => "待处理",
            WorkOrderStatus.Processing => "处理中",
            WorkOrderStatus.Closed => "已关闭",
            _ => "未知"
        };

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? Resolution { get; set; }

        /// <summary>是否已关闭
        /// （用于 XAML 绑定）
        /// </summary>
        public bool IsClosed => Status == WorkOrderStatus.Closed;

        /// <summary>是否待处理
        /// （用于 XAML 绑定）
        /// </summary>
        public bool IsPending => Status == WorkOrderStatus.Pending;

        /// <summary>
        /// 是否处理中（用于 XAML 绑定）
        /// </summary>
        public bool IsProcessing => Status == WorkOrderStatus.Processing;
    }
}
