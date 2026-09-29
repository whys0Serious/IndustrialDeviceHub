using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly IAlarmRepository _repo;
        private readonly ILogger<AlarmService> _logger;

        public AlarmService(IAlarmRepository repo, ILogger<AlarmService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task CreateAsync(int deviceId, AlarmType type, string message,
            double triggerValue, double thresholdValue, CancellationToken ct = default)
        {
            var alarm = new Alarm
            {
                DeviceId = deviceId,
                Type = type,
                Message = message,
                TriggerValue = triggerValue,
                ThresholdValue = thresholdValue,
                Status = AlarmStatus.Unacknowledged,
                CreatedAt = DateTime.Now
            };

            await _repo.AddAsync(alarm, ct);
            _logger.LogWarning("报警触发：{Type} - {Message}", type, message);
        }

        public async Task<IReadOnlyList<AlarmDto>> GetUnacknowledgedAsync(
            int limit = 50, CancellationToken ct = default)
        {
            var alarms = await _repo.GetUnacknowledgedAsync(limit, ct);
            return alarms.Select(ToDto).ToList();
        }

        public async Task<(IReadOnlyList<AlarmDto> Items, int TotalCount)> GetPagedAsync(
            AlarmStatus? status,
            AlarmType? type,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetPagedAsync(status, type, page, pageSize, ct);
            return (items.Select(ToDto).ToList(), total);
        }

        public async Task AcknowledgeAsync(int alarmId, string acknowledgedBy,
            CancellationToken ct = default)
        {
            var alarm = await _repo.GetByIdAsync(alarmId, ct)
                ?? throw new BusinessException($"报警 {alarmId} 不存在");

            if (alarm.Status == AlarmStatus.Acknowledged)
                return;

            alarm.Status = AlarmStatus.Acknowledged;
            alarm.AcknowledgedAt = DateTime.Now;
            alarm.AcknowledgedBy = acknowledgedBy;

            await _repo.UpdateAsync(alarm, ct);
            _logger.LogInformation("报警已确认：Id={Id}, By={By}", alarmId, acknowledgedBy);
        }

        public Task<bool> HasRecentAlarmAsync(int deviceId, AlarmType type,
            int seconds, CancellationToken ct = default)
            => _repo.HasRecentAlarmAsync(deviceId, type, seconds, ct);

        private static AlarmDto ToDto(Alarm a) => new()
        {
            Id = a.Id,
            DeviceId = a.DeviceId,
            DeviceName = a.Device?.Name,
            Type = a.Type,
            Message = a.Message,
            TriggerValue = a.TriggerValue,
            ThresholdValue = a.ThresholdValue,
            Status = a.Status,
            CreatedAt = a.CreatedAt,
            AcknowledgedAt = a.AcknowledgedAt,
            AcknowledgedBy = a.AcknowledgedBy
        };
        public async Task ResolveAsync(int deviceId, AlarmType type, CancellationToken ct = default)
        {
            var alarm = await _repo.GetActiveAlarmAsync(deviceId, type, ct);
            if (alarm == null) return;

            alarm.ResolvedAt = DateTime.Now;
            await _repo.UpdateAsync(alarm, ct);

            _logger.LogInformation("报警已恢复：Type={Type}", type);
        }
    }
}
