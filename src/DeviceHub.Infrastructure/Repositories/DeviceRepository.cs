using DeviceHub.Core.Common;
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
    /// <summary>
    /// 设备仓储实现
    /// </summary>
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DeviceHubDbContext _db;

        public DeviceRepository(DeviceHubDbContext db)
        {
            _db = db;
        }

        public async Task<(IReadOnlyList<Device> Items, int TotalCount)> GetPagedAsync(
            string? keyword,
            DeviceStatus? status,
            int? categoryId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _db.Devices
                .AsNoTracking()//只读查询
                .Include(d => d.Category)
                .AsQueryable();

            //关键字模糊匹配
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                query = query.Where(d => d.Name.Contains(k) || d.Code.Contains(k));
            }

            //按状态筛选
            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            //按分类筛选
            if (categoryId.HasValue)
            {
                query = query.Where(d => d.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Device?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Devices
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<Device> AddAsync(Device device, CancellationToken ct = default)
        {
            _db.Devices.Add(device);
            await _db.SaveChangesAsync(ct);
            return device;
        }

        public async Task UpdateAsync(Device device, CancellationToken ct = default)
        {
            _db.Devices.Update(device);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var device = await _db.Devices.FindAsync(new object[] { id }, ct);
            if (device == null) return;

            device.IsDeleted = true;
            device.DeletedAt = DateTime.Now;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _db.Devices.Where(d => d.Code == code);

            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return await query.AnyAsync(ct);
        }
    }
}
