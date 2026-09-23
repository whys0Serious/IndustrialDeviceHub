using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 设备信息数据
    /// </summary>
    public class DeviceDto
    {
        /// <summary>
        /// 设备主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 设备编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus Status { get; set; }

        /// <summary>
        /// 所属分类主键
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 所属分类名称
        /// 实体里是 DeviceCategory导航属性，这里只取 Name
        /// 避免 UI 直接拿到实体、也避免循环引用
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>创建时间</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>最后更新时间，可能为空</summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
