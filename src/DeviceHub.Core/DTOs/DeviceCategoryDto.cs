using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 设备分类
    /// </summary>
    public class DeviceCategoryDto
    {
        /// <summary>
        /// 分类主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述可空
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 该分类下设备数量
        /// </summary>
        public int DeviceCount { get; set; }
    }
}
