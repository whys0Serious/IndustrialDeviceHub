using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    public interface IWorkOrderRepository
    {
        Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetPagedAsync(
            string? keyword, WorkOrderStatus? status, WorkOrderPriority? priority,
            int? deviceId, int pageIndex, int pageSize,
            CancellationToken ct = default);

        Task<WorkOrder?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<WorkOrder?> GetByOrderNoAsync(string orderNo, CancellationToken ct = default);
        Task<WorkOrder> AddAsync(WorkOrder order, CancellationToken ct = default);
        Task UpdateAsync(WorkOrder order, CancellationToken ct = default);
        Task AddLogAsync(WorkOrderLog log, CancellationToken ct = default);

        /// <summary>
        /// 生成下一个工单号
        /// </summary>
        Task<string> GenerateOrderNoAsync(CancellationToken ct = default);

        /// <summary>
        /// 按关键词搜索历史工单（Title/Description/Resolution）
        /// </summary>
        Task<IReadOnlyList<WorkOrder>> SearchByKeywordsAsync(
            IReadOnlyList<string> keywords,
            int limit = 10,
            CancellationToken ct = default);
    }
}
