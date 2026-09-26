using DeviceHub.Core.Common;
using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 设备业务服务接口
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PagedResult<DeviceDto>> GetPagedAsync(DeviceQueryDto query, CancellationToken ct = default);
        /// <summary>
        /// ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DeviceDto?> GetByIdAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> CreateAsync(CreateDeviceDto request, CancellationToken ct = default);
        /// <summary>
        /// 更新设备
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(int id, UpdateDeviceDto request, CancellationToken ct = default);
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// 判断分类是否已存在
        /// </summary>
        /// <param name="code"></param>
        /// <param name="excludeId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);

        /// <summary>
        /// 获取设备状态统计
        /// </summary>
        Task<DeviceStatisticsDto> GetStatisticsAsync(CancellationToken ct = default);
    }
}
