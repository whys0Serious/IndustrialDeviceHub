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
        /// 设备状态枚举转换
        /// </summary>
        public string StatusText => Status switch
        {
            DeviceStatus.Stopped => "已停止",
            DeviceStatus.Running => "运行中",
            DeviceStatus.Alarm => "报警",
            _ => "未知"
        };

        /// <summary>
        /// 所属分类主键
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 所属分类名称
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间可能为空
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
