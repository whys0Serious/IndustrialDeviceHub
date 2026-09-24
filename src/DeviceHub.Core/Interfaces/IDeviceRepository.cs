using DeviceHub.Core.Common;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 设备仓储接口
    /// </summary>
    public interface IDeviceRepository
    {
        /// <summary>
        /// 分页查询设备（含分类导航属性）
        /// 支持关键字模糊匹配 Name/Code可按状态、分类筛选
        /// </summary>
        Task<(IReadOnlyList<Device> Items, int TotalCount)> GetPagedAsync(
            string? keyword,
            DeviceStatus? status,
            int? categoryId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>
        /// 按主键查询设备（含分类导航属性）
        /// </summary>
        Task<Device?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// 新增设备
        /// </summary>
        Task<Device> AddAsync(Device device, CancellationToken ct = default);

        /// <summary>
        /// 更新设备
        /// </summary>
        Task UpdateAsync(Device device, CancellationToken ct = default);

        /// <summary>
        /// 删除设备（软删除，设置 IsDeleted = true）
        /// </summary>
        Task DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// 判断设备编号是否已存在（用于唯一性校验）
        /// </summary>
        /// <param name="code">设备编号</param>
        /// <param name="excludeId">排除的Id</param>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    }
}
