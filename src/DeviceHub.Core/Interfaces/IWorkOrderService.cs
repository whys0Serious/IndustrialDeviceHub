using DeviceHub.Core.Common;
using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    public interface IWorkOrderService
    {
        Task<PagedResult<WorkOrderDto>> GetPagedAsync(WorkOrderQueryParams query, CancellationToken ct = default);
        Task<WorkOrderDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<WorkOrderLogDto>> GetLogsAsync(int orderId, CancellationToken ct = default);
        Task<int> CreateAsync(CreateWorkOrderRequest request, CancellationToken ct = default);
        Task UpdateAsync(int orderId, UpdateWorkOrderRequest request, CancellationToken ct = default);
        Task StartAsync(int orderId, string assignedTo, CancellationToken ct = default);
        Task CloseAsync(int orderId, string resolution, CancellationToken ct = default);
        Task AddLogAsync(int orderId, string action, string? remark, string? operatorName, CancellationToken ct = default);
    }
}
