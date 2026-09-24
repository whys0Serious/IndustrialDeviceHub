using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Repositories
{
    /// <summary>
    /// 设备分类仓储实现
    /// </summary>
    public class DeviceCategoryRepository : IDeviceCategoryRepository
    {
        private readonly DeviceHubDbContext _db;

        public DeviceCategoryRepository(DeviceHubDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<DeviceCategory>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.DeviceCategories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<DeviceCategory?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.DeviceCategories
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<DeviceCategory> AddAsync(DeviceCategory category, CancellationToken ct = default)
        {
            _db.DeviceCategories.Add(category);
            await _db.SaveChangesAsync(ct);
            return category;
        }

        public async Task UpdateAsync(DeviceCategory category, CancellationToken ct = default)
        {
            _db.DeviceCategories.Update(category);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var category = await _db.DeviceCategories.FindAsync(new object[] { id }, ct);
            if (category == null) return;

            _db.DeviceCategories.Remove(category);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> HasDevicesAsync(int categoryId, CancellationToken ct = default)
        {
            //只统计未删除的设备
            return await _db.Devices.AnyAsync(d => d.CategoryId == categoryId, ct);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _db.DeviceCategories.Where(c => c.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(ct);
        }
    }
}
