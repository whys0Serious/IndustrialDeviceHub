using DeviceHub.Core.Common;
using DeviceHub.Core.DTOs;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Exceptions;
using DeviceHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 设备业务服务实现
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repo;
        private readonly IDeviceCategoryRepository _categoryRepo;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IDeviceRepository repo, ILogger<DeviceService> logger, IDeviceCategoryRepository categoryRepo)
        {
            _repo = repo;
            _logger = logger;
            _categoryRepo = categoryRepo;
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
        {   //编号唯一
            if (await _repo.CodeExistsAsync(request.Code, null, ct))
                throw new BusinessException($"设备编号「{request.Code}」已存在");

            //分类存在
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId, ct);
            if (category == null)
                throw new BusinessException($"分类 {request.CategoryId} 不存在，请先在分类管理中创建");

            //构造实体
            var device = new Device
            {
                Name = request.Name.Trim(),
                Code = request.Code.Trim(),
                Status = request.Status,
                CategoryId = request.CategoryId,
                Remark = request.Remark?.Trim(),
                CreatedAt = DateTime.Now,
                ModbusSlaveId= request.ModbusSlaveId,
                ModbusStartAddress = request.ModbusStartAddress,
                ModbusRegisterCount = request.ModbusRegisterCount,
                EnableMonitoring = request.EnableMonitoring
            };

            //保存
            try
            {
                var saved = await _repo.AddAsync(device, ct);
                _logger.LogInformation("设备创建成功：Id={Id}, Code={Code}", saved.Id, saved.Code);
                return saved.Id;
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "设备创建失败：{Message}", innerMessage);

                if (innerMessage.Contains("IX_Devices_Code"))
                    throw new BusinessException($"设备编号「{request.Code}」已存在");
                if (innerMessage.Contains("FK_Devices_DeviceCategories"))
                    throw new BusinessException("分类不存在或已被删除");

                throw;//其他数据库异常往上抛 全局异常兜底
            }
        }

        public async Task UpdateAsync(int id, UpdateDeviceDto request, CancellationToken ct = default)
        {
            var device = await _repo.GetByIdAsync(id, ct)
                ?? throw new BusinessException($"设备 {id} 不存在");

            if (await _repo.CodeExistsAsync(request.Code, id, ct))
            {
                throw new BusinessException($"设备编号 {request.Code} 已被其他设备使用");
            }

            device.Name = request.Name.Trim();
            device.Code = request.Code.Trim();
            device.Status = request.Status;
            device.CategoryId = request.CategoryId;
            device.Remark = request.Remark?.Trim();
            device.UpdatedAt = DateTime.Now;
            device.EnableMonitoring=request.EnableMonitoring;
            device.ModbusSlaveId = request.ModbusSlaveId;
            device.ModbusStartAddress = request.ModbusStartAddress;
            device.ModbusRegisterCount = request.ModbusRegisterCount;

            try
            {
                await _repo.UpdateAsync(device, ct);
                _logger.LogInformation("设备更新成功：Id={Id}", id);
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "设备更新失败：{Message}", innerMessage);

                if (innerMessage.Contains("IX_Devices_Code"))
                    throw new BusinessException($"设备编号「{request.Code}」已存在");

                throw;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var device = await _repo.GetByIdAsync(id, ct)
                ?? throw new BusinessException($"设备 {id} 不存在");

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
                UpdatedAt = device.UpdatedAt,
                ModbusSlaveId = device.ModbusSlaveId,
                ModbusStartAddress = device.ModbusStartAddress,
                ModbusRegisterCount = device.ModbusRegisterCount,
                EnableMonitoring = device.EnableMonitoring
            };
        }

        public async Task<DeviceStatisticsDto> GetStatisticsAsync(CancellationToken ct = default)
        {
            var (running, alarm, stopped) = await _repo.GetStatusStatisticsAsync(ct);

            return new DeviceStatisticsDto
            {
                TotalCount = running + alarm + stopped,
                RunningCount = running,
                AlarmCount = alarm,
                StoppedCount = stopped
            };
        }
    }
}
