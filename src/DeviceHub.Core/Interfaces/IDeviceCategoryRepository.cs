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
            Task<IReadOnlyList<DeviceCategory>> GetAllAsync(CancellationToken ct = default);

            /// <summary>
            /// 按主键查询分类 找不到返回 null
            /// </summary>
            Task<DeviceCategory?> GetByIdAsync(int id, CancellationToken ct = default);

            /// <summary>
            /// 新增分类
            /// </summary>
            Task<DeviceCategory> AddAsync(DeviceCategory category, CancellationToken ct = default);

            /// <summary>
            /// 更新分类
            /// </summary>
            Task UpdateAsync(DeviceCategory category, CancellationToken ct = default);

            /// <summary>
            /// 删除分类（软删除）
            /// </summary>
            Task DeleteAsync(int id, CancellationToken ct = default);

            /// <summary>
            /// 判断分类下是否有设备
            /// </summary>
            Task<bool> HasDevicesAsync(int categoryId, CancellationToken ct = default);

            /// <summary>
            /// 判断分类名是否已存在
            /// </summary>
            Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
        }
    }
