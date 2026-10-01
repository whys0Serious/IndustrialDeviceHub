using DeviceHub.Core.Common;
using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class WorkOrderQueryParams : PagedRequest
    {
        public string? Keyword { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public WorkOrderPriority? Priority { get; set; }
        public int? DeviceId { get; set; }
    }
}
