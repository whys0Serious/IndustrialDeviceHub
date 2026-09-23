using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 更新设备分类入参。
    /// 字段与 Create一致，单独定义便于将来差异化
    /// </summary>
    public class UpdateDeviceCategoryDto
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
