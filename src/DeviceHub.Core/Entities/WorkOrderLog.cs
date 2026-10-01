using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Entities
{
    /// <summary>
    /// 工单处理记录
    /// </summary>
    public class WorkOrderLog
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联工单Id
        /// </summary>
        public int WorkOrderId { get; set; }

        /// <summary>
        /// 工单（导航属性）
        /// </summary>
        public WorkOrder? WorkOrder { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
