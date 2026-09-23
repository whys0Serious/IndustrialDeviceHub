using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Entities
{
    /// <summary>
    /// 设备分类
    /// </summary>
    public class DeviceCategory
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 分类名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 关联设备导航，一对多
        /// </summary>
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
