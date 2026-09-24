using DeviceHub.Core.Common;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 设备业务服务实现
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repo;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IDeviceRepository repo, ILogger<DeviceService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<PagedResult<DeviceDto>> GetPagedAsync(DeviceQueryDto query, CancellationToken ct = default)
        {
            var (items, totalCount) = await _repo.GetPagedAsync(
                query.Keyword,
                query.Status,
                query.CategoryId,
                query.Page,
                query.PageSize,
                ct);

            return new PagedResult<DeviceDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task<DeviceDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var device = await _repo.GetByIdAsync(id, ct);
            return device == null ? null : ToDto(device);
        }

        public async Task<int> CreateAsync(CreateDeviceDto request, CancellationToken ct = default)
        {
            if (await _repo.CodeExistsAsync(request.Code, null, ct))
            {
                throw new InvalidOperationException($"设备编号 {request.Code} 已存在");
            }

            var device = new Device
            {
                Name = request.Name.Trim(),
                Code = request.Code.Trim(),
                Status = request.Status,
                CategoryId = request.CategoryId,
                Remark = request.Remark?.Trim(),
                CreatedAt = DateTime.Now
            };

            var saved = await _repo.AddAsync(device, ct);

            _logger.LogInformation("设备创建成功：Id={Id}, Code={Code}", saved.Id, saved.Code);

            return saved.Id;
        }

        public async Task UpdateAsync(int id, UpdateDeviceDto request, CancellationToken ct = default)
        {
            var device = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException($"设备 {id} 不存在");

            if (await _repo.CodeExistsAsync(request.Code, id, ct))
            {
                throw new InvalidOperationException($"设备编号 {request.Code} 已被其他设备使用");
            }

            device.Name = request.Name.Trim();
            device.Code = request.Code.Trim();
            device.Status = request.Status;
            device.CategoryId = request.CategoryId;
            device.Remark = request.Remark?.Trim();
            device.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(device, ct);

            _logger.LogInformation("设备更新成功：Id={Id}", id);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var device = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException($"设备 {id} 不存在");

            await _repo.DeleteAsync(id, ct);

            _logger.LogInformation("设备删除成功：Id={Id}", id);
        }

        public Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        {
            return _repo.CodeExistsAsync(code, excludeId, ct);
        }

        private static DeviceDto ToDto(Device device)
        {
            return new DeviceDto
            {
                Id = device.Id,
                Name = device.Name,
                Code = device.Code,
                Status = device.Status,
                CategoryId = device.CategoryId,
                CategoryName = device.Category?.Name,
                Remark = device.Remark,
                CreatedAt = device.CreatedAt,
                UpdatedAt = device.UpdatedAt
            };
        }
    }
}
