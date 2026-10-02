using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Entities
{
    /// <summary>
    /// 设备信息
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 设备名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>
        /// 状态
        /// </summary>
        public DeviceStatus Status { get; set; }
        /// <summary>
        /// 分类外键关联
        /// </summary>
        public int CategoryId { get; set; }
        public DeviceCategory? Category { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        /// <summary>
        /// 软删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 软删除时间
        /// </summary>
        public DateTime? DeletedAt { get; set; }


        /// <summary>
        /// Modbus从站ID（0-247）
        /// </summary>
        public byte? ModbusSlaveId { get; set; }

        /// <summary>
        /// Modbus起始寄存器地址
        /// </summary>
        public ushort? ModbusStartAddress { get; set; }

        /// <summary>
        /// Modbus寄存器数量
        /// </summary>
        public ushort? ModbusRegisterCount { get; set; }

        /// <summary>
        /// 是否启用实时监控
        /// </summary>
        public bool EnableMonitoring { get; set; } = false;
    }
}
