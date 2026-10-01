using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Entities
{
    /// <summary>
    /// 工单
    /// </summary>
    public class WorkOrder
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 关联设备Id
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 设备（导航属性）
        /// </summary>
        public Device? Device { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 故障描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 优先级
        /// </summary>
        public WorkOrderPriority Priority { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public WorkOrderStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        public string? AssignedTo { get; set; }

        /// <summary>
        /// 开始处理时间
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 处理结果
        /// </summary>
        public string? Resolution { get; set; }

        /// <summary
        /// >处理记录（一对多）
        /// </summary>
        public ICollection<WorkOrderLog> Logs { get; set; } = new List<WorkOrderLog>();
    }
}
