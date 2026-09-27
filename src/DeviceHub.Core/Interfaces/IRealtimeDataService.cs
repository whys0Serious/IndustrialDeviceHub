using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 实时数据查询服务
    /// </summary>
    public interface IRealtimeDataService
    {
        /// <summary>
        /// 获取指定设备的最新数据
        /// </summary>
        RealtimeDataDto? GetLatest(int deviceId);

        /// <summary>
        /// 获取所有设备的最新数据
        /// </summary>
        IReadOnlyCollection<RealtimeDataDto> GetAllLatest();

        /// <summary>
        /// Modbus是否已连接
        /// </summary>
        bool IsConnected { get; }
    }
}
