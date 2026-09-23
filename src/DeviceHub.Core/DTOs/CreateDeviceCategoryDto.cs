using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 新增设备分类参数
    /// </summary>
    public class CreateDeviceCategoryDto
    {
        /// <summary>
        /// 分类名称必填且唯一
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述可空
        /// </summary>
        public string? Description { get; set; }
    }
}
