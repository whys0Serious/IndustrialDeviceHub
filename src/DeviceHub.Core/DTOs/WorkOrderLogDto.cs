using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    public class WorkOrderLogDto
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string? Operator { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
