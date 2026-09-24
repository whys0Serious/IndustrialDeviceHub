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
    /// 设备分类业务服务实现
    /// </summary>
    public class DeviceCategoryService : IDeviceCategoryService
    {
        private readonly IDeviceCategoryRepository _repo;
        private readonly ILogger<DeviceCategoryService> _logger;

        public DeviceCategoryService(IDeviceCategoryRepository repo, ILogger<DeviceCategoryService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<DeviceCategoryDto>> GetAllAsync(CancellationToken ct = default)
        {
            var categories = await _repo.GetAllAsync(ct);
            return categories.Select(ToDto).ToList();
        }

        public async Task<DeviceCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var category = await _repo.GetByIdAsync(id, ct);
            return category == null ? null : ToDto(category);
        }

        public async Task<int> CreateAsync(CreateDeviceCategoryDto request, CancellationToken ct = default)
        {
            if (await _repo.NameExistsAsync(request.Name, null, ct))
            {
                throw new InvalidOperationException($"分类名 {request.Name} 已存在");
            }

            var category = new DeviceCategory
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };

            var saved = await _repo.AddAsync(category, ct);

            _logger.LogInformation("分类创建成功：Id={Id}, Name={Name}", saved.Id, saved.Name);

            return saved.Id;
        }

        public async Task UpdateAsync(int id, UpdateDeviceCategoryDto request, CancellationToken ct = default)
        {
            var category = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException($"分类 {id} 不存在");

            if (await _repo.NameExistsAsync(request.Name, id, ct))
            {
                throw new InvalidOperationException($"分类名 {request.Name} 已被使用");
            }

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();

            await _repo.UpdateAsync(category, ct);

            _logger.LogInformation("分类更新成功：Id={Id}", id);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var category = await _repo.GetByIdAsync(id, ct)
                ?? throw new InvalidOperationException($"分类 {id} 不存在");

            //分类下有设备时禁止删除
            if (await _repo.HasDevicesAsync(id, ct))
            {
                throw new InvalidOperationException($"分类 {category.Name} 下还有设备，不能删除");
            }

            await _repo.DeleteAsync(id, ct);

            _logger.LogInformation("分类删除成功：Id={Id}", id);
        }

        public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        {
            return _repo.NameExistsAsync(name, excludeId, ct);
        }

        private static DeviceCategoryDto ToDto(DeviceCategory category)
        {
            return new DeviceCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DeviceCount = category.Devices?.Count ?? 0
            };
        }
    }
}
