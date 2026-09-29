using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Models
{
    /// <summary>
    /// Modbus配置
    /// </summary>
    public class ModbusOptions
    {
        public const string SectionName = "Modbus";

        /// <summary>
        /// 是否启用轮询
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// IP地址
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 502;

        /// <summary>
        /// 从站ID
        /// </summary>
        public byte SlaveId { get; set; } = 1;

        /// <summary>
        /// 起始寄存器地址
        /// </summary>
        public ushort StartAddress { get; set; } = 0;

        /// <summary>
        /// 读取寄存器数量
        /// </summary>
        public ushort RegisterCount { get; set; } = 10;

        /// <summary>
        /// 轮询间隔（毫秒）
        /// </summary>
        public int PollingIntervalMs { get; set; } = 1000;
    }
}
