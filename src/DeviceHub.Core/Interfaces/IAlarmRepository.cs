using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 报警记录接口
    /// </summary>
    public interface IAlarmRepository
    {
        /// <summary>
        /// 添加报警
        /// </summary>
        Task<Alarm> AddAsync(Alarm alarm, CancellationToken ct = default);

        /// <summary>
        /// 获取未确认的报警
        /// </summary>
        Task<IReadOnlyList<Alarm>> GetUnacknowledgedAsync(int limit = 50, CancellationToken ct = default);

        /// <summary>
        /// 分页查询
        /// </summary>
        Task<(IReadOnlyList<Alarm> Items, int TotalCount)> GetPagedAsync(
            AlarmStatus? status,
            AlarmType? type,
            int page,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// 按Id查询
        /// </summary>
        Task<Alarm?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// 更新
        /// </summary>
        Task UpdateAsync(Alarm alarm, CancellationToken ct = default);

        /// <summary>
        /// 检查最近N秒内是否有同类报警
        /// </summary>
        Task<bool> HasRecentAlarmAsync(
            int deviceId, AlarmType type, int seconds, CancellationToken ct = default);

        /// <summary>
        /// 获取指定设备的指定类型的活动报警（未恢复的）
        /// 恢复时找到对应的报警记录
        /// </summary>
        Task<Alarm?> GetActiveAlarmAsync(int deviceId, AlarmType type, CancellationToken ct = default);
    }
}
