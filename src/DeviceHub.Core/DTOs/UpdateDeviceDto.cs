using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 更新设备参数
    /// </summary>
    public class UpdateDeviceDto
    {
        /// <summary>
        /// 设备名称必填
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 设备编码必填且唯一
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
    }
}
