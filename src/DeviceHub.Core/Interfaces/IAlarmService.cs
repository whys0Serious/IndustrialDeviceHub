using DeviceHub.Core.DTOs;
using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    public interface IAlarmService
    {
        Task CreateAsync(int deviceId, AlarmType type, string message,
            double triggerValue, double thresholdValue, CancellationToken ct = default);

        Task<IReadOnlyList<AlarmDto>> GetUnacknowledgedAsync(int limit = 50, CancellationToken ct = default);

        Task<(IReadOnlyList<AlarmDto> Items, int TotalCount)> GetPagedAsync(
            AlarmStatus? status,
            AlarmType? type,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task AcknowledgeAsync(int alarmId, string acknowledgedBy, CancellationToken ct = default);

        Task<bool> HasRecentAlarmAsync(int deviceId, AlarmType type,
            int seconds, CancellationToken ct = default);

        Task ResolveAsync(int deviceId, AlarmType type, CancellationToken ct = default);
    }
}
