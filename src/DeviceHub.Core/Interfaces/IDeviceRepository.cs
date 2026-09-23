using DeviceHub.Core.Common;
using DeviceHub.Core.Entities;
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
        /// 分页查询设备
        /// 支持关键字模糊匹配 Name / Code。
        /// </summary>
        Task<PagedResult<Device>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

        /// <summary>
        /// 按主键查询设备包含分类导航属性
        /// 找不到返回 null
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
        /// 按主键删除设备
        /// </summary>
        Task DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// 判断设备编码是否已存在
        /// </summary>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    }
}
