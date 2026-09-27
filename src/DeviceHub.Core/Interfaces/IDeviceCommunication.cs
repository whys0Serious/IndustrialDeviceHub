using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 设备通信接口：抽象读/写寄存器的能力，屏蔽底层协议差异
    /// 目前用Modbus TCP实现，未来可替换为OPC UA、S7等
    /// </summary>
    public interface IDeviceCommunication : IDisposable
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="host">IP 地址或主机名</param>
        /// <param name="port">端口（Modbus TCP 默认 502）</param>
        Task<bool> ConnectAsync(string host, int port, CancellationToken ct = default);

        /// <summary>
        /// 断开连接
        /// </summary>
        Task DisconnectAsync(CancellationToken ct = default);

        /// <summary>
        /// 读保持寄存器
        /// </summary>
        /// <param name="slaveId">从站地址（Modbus从站ID）</param>
        /// <param name="startAddress">起始地址（0-based）</param>
        /// <param name="count">寄存器数量</param>
        Task<ushort[]> ReadHoldingRegistersAsync(
            byte slaveId,
            ushort startAddress,
            ushort count,
            CancellationToken ct = default);

        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        Task WriteSingleRegisterAsync(
            byte slaveId,
            ushort address,
            ushort value,
            CancellationToken ct = default);
    }
}
