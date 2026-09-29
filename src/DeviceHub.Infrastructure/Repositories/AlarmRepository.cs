using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly DeviceHubDbContext _db;

        public AlarmRepository(DeviceHubDbContext db)
        {
            _db = db;
        }

        public async Task<Alarm> AddAsync(Alarm alarm, CancellationToken ct = default)
        {
            _db.Alarms.Add(alarm);
            await _db.SaveChangesAsync(ct);
            return alarm;
        }

        public async Task<IReadOnlyList<Alarm>> GetUnacknowledgedAsync(int limit = 50, CancellationToken ct = default)
        {
            return await _db.Alarms
                .AsNoTracking()
                .Include(a => a.Device)
                .Where(a => a.Status == AlarmStatus.Unacknowledged)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<Alarm> Items, int TotalCount)> GetPagedAsync(
            AlarmStatus? status,
            AlarmType? type,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _db.Alarms
                .AsNoTracking()
                .Include(a => a.Device)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<Alarm?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Alarms
                .Include(a => a.Device)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task UpdateAsync(Alarm alarm, CancellationToken ct = default)
        {
            _db.Alarms.Update(alarm);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> HasRecentAlarmAsync(
            int deviceId, AlarmType type, int seconds, CancellationToken ct = default)
        {
            var since = DateTime.Now.AddSeconds(-seconds);
            return await _db.Alarms
                .AnyAsync(a => a.DeviceId == deviceId
                            && a.Type == type
                            && a.CreatedAt >= since, ct);
        }

        public async Task<Alarm?> GetActiveAlarmAsync(
            int deviceId, AlarmType type, CancellationToken ct = default)
        {
            //活动=已触发但未恢复
            return await _db.Alarms
                .Where(a => a.DeviceId == deviceId
                         && a.Type == type
                         && a.ResolvedAt == null)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}
