using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 设备查询参数
    /// </summary>
    public class DeviceQueryDto
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 关键字（匹配名称或编号）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 按状态筛选
        /// </summary>
        public DeviceStatus? Status { get; set; }

        /// <summary
        /// >按分类筛选
        /// </summary>
        public int? CategoryId { get; set; }
    }
}
