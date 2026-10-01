using DeviceHub.Core.Common;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using DeviceHub.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(
            IWorkOrderRepository repo,
            IDeviceRepository deviceRepo,
            ILogger<WorkOrderService> logger)
        {
            _repo = repo;
            _deviceRepo = deviceRepo;
            _logger = logger;
        }

        public async Task<PagedResult<WorkOrderDto>> GetPagedAsync(
            WorkOrderQueryParams query, CancellationToken ct = default)
        {
            var (items, total) = await _repo.GetPagedAsync(
                query.Keyword, query.Status, query.Priority, query.DeviceId,
                query.PageIndex, query.PageSize, ct);

            return new PagedResult<WorkOrderDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = total,
                Page = query.PageIndex,
                PageSize = query.PageSize
            };
        }

        public async Task<WorkOrderDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(id, ct);
            return order == null ? null : ToDto(order);
        }

        public async Task<IReadOnlyList<WorkOrderLogDto>> GetLogsAsync(
            int orderId, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(orderId, ct)
                ?? throw new BusinessException($"工单 {orderId} 不存在");

            return order.Logs
                .OrderByDescending(l => l.CreatedAt)
                .Select(ToLogDto)
                .ToList();
        }

        public async Task<int> CreateAsync(CreateWorkOrderRequest request, CancellationToken ct = default)
        {
            var device = await _deviceRepo.GetByIdAsync(request.DeviceId, ct)
                ?? throw new BusinessException($"设备 {request.DeviceId} 不存在");

            var orderNo = await _repo.GenerateOrderNoAsync(ct);

            var order = new WorkOrder
            {
                OrderNo = orderNo,
                DeviceId = request.DeviceId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Priority = request.Priority,
                Status = WorkOrderStatus.Pending,
                CreatedAt = DateTime.Now,
                CreatedBy = request.CreatedBy ?? "admin"
            };

            var saved = await _repo.AddAsync(order, ct);

            await _repo.AddLogAsync(new WorkOrderLog
            {
                WorkOrderId = saved.Id,
                Action = "创建工单",
                Remark = $"工单 {orderNo} 已创建",
                Operator = order.CreatedBy,
                CreatedAt = DateTime.Now
            }, ct);

            _logger.LogInformation("工单创建成功：Id={Id}, No={No}", saved.Id, orderNo);
            return saved.Id;
        }

        public async Task UpdateAsync(int orderId, UpdateWorkOrderRequest request, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(orderId, ct)
                ?? throw new BusinessException($"工单 {orderId} 不存在");

            if (order.Status == WorkOrderStatus.Closed)
                throw new BusinessException("工单已关闭，不能修改");

            order.Title = request.Title.Trim();
            order.Description = request.Description.Trim();
            order.Priority = request.Priority;
            order.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(order, ct);
            _logger.LogInformation("工单更新：Id={Id}", orderId);
        }

        public async Task StartAsync(int orderId, string assignedTo, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(orderId, ct)
                ?? throw new BusinessException($"工单 {orderId} 不存在");

            WorkOrderStateMachine.EnsureCanTransition(order.Status, WorkOrderStatus.Processing);

            order.Status = WorkOrderStatus.Processing;
            order.AssignedTo = assignedTo;
            order.StartedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(order, ct);

            await _repo.AddLogAsync(new WorkOrderLog
            {
                WorkOrderId = order.Id,
                Action = "开始处理",
                Remark = $"由 {assignedTo} 接手处理",
                Operator = assignedTo,
                CreatedAt = DateTime.Now
            }, ct);

            _logger.LogInformation("工单开始处理：Id={Id}", orderId);
        }

        public async Task CloseAsync(int orderId, string resolution, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(orderId, ct)
                ?? throw new BusinessException($"工单 {orderId} 不存在");

            WorkOrderStateMachine.EnsureCanTransition(order.Status, WorkOrderStatus.Closed);

            order.Status = WorkOrderStatus.Closed;
            order.ClosedAt = DateTime.Now;
            order.Resolution = resolution.Trim();
            order.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(order, ct);

            await _repo.AddLogAsync(new WorkOrderLog
            {
                WorkOrderId = order.Id,
                Action = "关闭工单",
                Remark = $"处理结果：{resolution}",
                Operator = order.AssignedTo,
                CreatedAt = DateTime.Now
            }, ct);

            _logger.LogInformation("工单已关闭：Id={Id}", orderId);
        }

        public async Task AddLogAsync(int orderId, string action, string? remark,
            string? operatorName, CancellationToken ct = default)
        {
            var order = await _repo.GetByIdAsync(orderId, ct)
                ?? throw new BusinessException($"工单 {orderId} 不存在");

            await _repo.AddLogAsync(new WorkOrderLog
            {
                WorkOrderId = orderId,
                Action = action,
                Remark = remark,
                Operator = operatorName,
                CreatedAt = DateTime.Now
            }, ct);
        }

        private static WorkOrderDto ToDto(WorkOrder o) => new()
        {
            Id = o.Id,
            OrderNo = o.OrderNo,
            DeviceId = o.DeviceId,
            DeviceName = o.Device?.Name,
            Title = o.Title,
            Description = o.Description,
            Priority = o.Priority,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            CreatedBy = o.CreatedBy,
            UpdatedAt = o.UpdatedAt,
            AssignedTo = o.AssignedTo,
            StartedAt = o.StartedAt,
            ClosedAt = o.ClosedAt,
            Resolution = o.Resolution
        };

        private static WorkOrderLogDto ToLogDto(WorkOrderLog l) => new()
        {
            Id = l.Id,
            WorkOrderId = l.WorkOrderId,
            Action = l.Action,
            Remark = l.Remark,
            Operator = l.Operator,
            CreatedAt = l.CreatedAt
        };
    }
}
