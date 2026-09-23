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

        /// <summary>
        /// 查询所有分类按名称排序
        /// 分类数量少不做分页
        /// </summary>
        public async Task<IReadOnlyList<DeviceCategory>> GetAllCategory(
            CancellationToken ct = default)
        {
            return await _db.DeviceCategories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(ct);
        }

        /// <summary>
        /// 按主键查询分类
        /// </summary>
        public Task<DeviceCategory?> GetCategoryById(
            int id,
            CancellationToken ct = default)
        {
            return _db.DeviceCategories
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        /// <summary>
        /// 新增分类
        /// </summary>
        public async Task<DeviceCategory> AddCategory(
            DeviceCategory category,
            CancellationToken ct = default)
        {
            _db.DeviceCategories.Add(category);
            await _db.SaveChangesAsync(ct);
            return category;
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        public async Task UpdateCategory(
            DeviceCategory category,
            CancellationToken ct = default)
        {
            _db.DeviceCategories.Update(category);
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 按主键删除分类
        /// 判断是否有关联设备
        /// </summary>
        public async Task DeleteCategory(int id, CancellationToken ct = default)
        {
            var entity = await _db.DeviceCategories.FindAsync(new object?[] { id }, ct);
            if (entity is null)
            {
                return;
            }

            _db.DeviceCategories.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 判断分类下是否有设备
        /// 有关联设备则拒绝删除
        /// </summary>
        public Task<bool> IsHasDevices(
            int categoryId,
            CancellationToken ct = default)
        {
            return _db.Devices.AnyAsync(x => x.CategoryId == categoryId, ct);
        }
    }
}
