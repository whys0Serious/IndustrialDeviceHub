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
    public class WorkOrderRepository : IWorkOrderRepository
    {
        private readonly DeviceHubDbContext _db;

        public WorkOrderRepository(DeviceHubDbContext db)
        {
            _db = db;
        }

        public async Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPagedAsync(
            string? keyword, WorkOrderStatus? status, WorkOrderPriority? priority,
            int? deviceId, int pageIndex, int pageSize, CancellationToken ct = default)
        {
            var query = _db.WorkOrders
                .AsNoTracking()
                .Include(o => o.Device)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                query = query.Where(o => o.OrderNo.Contains(k)
                                      || o.Title.Contains(k)
                                      || o.Description.Contains(k));
            }

            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            if (priority.HasValue) query = query.Where(o => o.Priority == priority.Value);
            if (deviceId.HasValue) query = query.Where(o => o.DeviceId == deviceId.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.WorkOrders
                .Include(o => o.Device)
                .Include(o => o.Logs)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<WorkOrder?> GetByOrderNoAsync(string orderNo, CancellationToken ct = default)
        {
            return await _db.WorkOrders.FirstOrDefaultAsync(o => o.OrderNo == orderNo, ct);
        }

        public async Task<WorkOrder> AddAsync(WorkOrder order, CancellationToken ct = default)
        {
            _db.WorkOrders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order;
        }

        public async Task UpdateAsync(WorkOrder order, CancellationToken ct = default)
        {
            _db.WorkOrders.Update(order);
            await _db.SaveChangesAsync(ct);
        }

        public async Task AddLogAsync(WorkOrderLog log, CancellationToken ct = default)
        {
            _db.WorkOrderLogs.Add(log);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<string> GenerateOrderNoAsync(CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var prefix = $"WO-{today:yyyyMMdd}-";

            var todayCount = await _db.WorkOrders
                .Where(o => o.OrderNo.StartsWith(prefix))
                .CountAsync(ct);

            var seq = (todayCount + 1).ToString("D3");
            return $"{prefix}{seq}";
        }

        public async Task<IReadOnlyList<WorkOrder>> SearchByKeywordsAsync(
        IReadOnlyList<string> keywords,
        int limit = 10,
        CancellationToken ct = default)
        {
            if (keywords == null || keywords.Count == 0)
                return Array.Empty<WorkOrder>();

            var query = _db.WorkOrders
                .AsNoTracking()
                .Include(o => o.Device)
                .Where(o => o.Status == WorkOrderStatus.Closed);   //只搜已关闭的（有处理结果）

            //只匹配Title
            query = query.Where(o => keywords.Any(k => o.Title.Contains(k)));

            return await query
                .OrderByDescending(o => o.ClosedAt)
                .Take(limit)
                .ToListAsync(ct);
        }
    }
}
