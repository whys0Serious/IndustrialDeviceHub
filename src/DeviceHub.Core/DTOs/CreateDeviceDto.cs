using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.DTOs
{
    /// <summary>
    /// 新增设备参数
    /// </summary>
    public class CreateDeviceDto
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
        /// 所属分类主键必填
        /// </summary>
        public int CategoryId { get; set; }
    }
}
