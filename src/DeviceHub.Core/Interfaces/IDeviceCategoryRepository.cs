using DeviceHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 设备分类仓储接口
    /// </summary>
    public interface IDeviceCategoryRepository
    {
        /// <summary>
        /// 查询所有分类按名称排序
        /// </summary>
        Task<IReadOnlyList<DeviceCategory>> GetAllCategory(CancellationToken ct = default);

        /// <summary>
        /// 按主键查询分类
        /// 找不到返回null
        /// </summary>
        Task<DeviceCategory?> GetCategoryById(int id, CancellationToken ct = default);

        /// <summary>
        /// 新增分类
        /// </summary>
        Task<DeviceCategory> AddCategory(DeviceCategory category, CancellationToken ct = default);

        /// <summary>
        /// 更新分类
        /// </summary>
        Task UpdateCategory(DeviceCategory category, CancellationToken ct = default);

        /// <summary>
        /// 按主键删除分类
        /// </summary>
        Task DeleteCategory(int id, CancellationToken ct = default);

        /// <summary>
        /// 判断分类下是否有设备
        /// </summary>
        Task<bool> IsHasDevices(int categoryId, CancellationToken ct = default);
    }
}
