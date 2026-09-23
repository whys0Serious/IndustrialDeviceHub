using DeviceHub.Core.Common;
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
    /// 设备仓储实现
    /// </summary>
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DeviceHubDbContext _db;

        /// <summary>
        /// 构造函数注入 DbContext。
        /// DbContext 由 DI 管理，生命周期 Scoped。
        /// </summary>
        public DeviceRepository(DeviceHubDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 分页查询设备
        /// 支持关键字模糊匹配 Name / Code
        /// AsNoTracking提升只读性能
        /// </summary>
        public async Task<PagedResult<Device>> GetPagedDevice(
            PagedRequest request,
            CancellationToken ct = default)
        {
            // 1. 基础查询：包含分类，只读
            var query = _db.Devices
                .Include(x => x.Category)
                .AsNoTracking()
                .AsQueryable();

            // 2. 关键字过滤
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var kw = request.Keyword.Trim();
                query = query.Where(x => x.Name.Contains(kw) || x.Code.Contains(kw));
            }

            // 3. 统计总数
            var total = await query.CountAsync(ct);

            // 4. 分页取数据
            var items = await query
                .OrderByDescending(x => x.Id)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            // 5. 组装返回
            return new PagedResult<Device>
            {
                Items = items,
                TotalCount = total,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }

        /// <summary>
        /// 按主键查询设备包含分类
        /// 找不到返回 null不抛异常
        /// </summary>
        public Task<Device?> GetDeviceById(int id, CancellationToken ct = default)
        {
            return _db.Devices
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        /// <summary>
        /// 新增设备
        /// 返回带数据库生成主键的实体
        /// </summary>
        public async Task<Device> AddDevice(Device device, CancellationToken ct = default)
        {
            device.CreatedAt = DateTime.UtcNow;
            _db.Devices.Add(device);
            await _db.SaveChangesAsync(ct);
            return device;
        }

        /// <summary>
        /// 更新设备
        /// </summary>
        public async Task UpdateDevice(Device device, CancellationToken ct = default)
        {
            device.UpdatedAt = DateTime.UtcNow;
            _db.Devices.Update(device);
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 按主键删除设备
        /// </summary>
        public async Task DeleteDevice(int id, CancellationToken ct = default)
        {
            var entity = await _db.Devices.FindAsync(new object?[] { id }, ct);
            if (entity is null)
            {
                return;
            }

            _db.Devices.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 判断设备编码是否已存在
        /// </summary>
        public Task<bool> CodeExistsDevice(
            string code,
            int? excludeId = null,
            CancellationToken ct = default)
        {
            return _db.Devices.AnyAsync(
                x => x.Code == code && (excludeId == null || x.Id != excludeId),
                ct);
        }
    }
}
