using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 实时数据缓存
    /// </summary>
    public interface IRealtimeDataCache
    {
        /// <summary>
        /// 更新设备的实时数据
        /// </summary>
        void Update(int deviceId, RealtimeDataDto data);

        /// <summary>
        /// 获取指定设备的最新数据
        /// </summary>
        RealtimeDataDto? Get(int deviceId);

        /// <summary>
        /// 获取所有设备的最新数据
        /// </summary>
        IReadOnlyCollection<RealtimeDataDto> GetAll();

        /// <summary>
        /// 清空
        /// </summary>
        void Clear();
    }
}
