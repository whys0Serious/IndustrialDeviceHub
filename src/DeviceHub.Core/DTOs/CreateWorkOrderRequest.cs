using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class CreateWorkOrderRequest
    {
        public int DeviceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;

        /// <summary>
        /// 创建人（登录当前用户）
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}
