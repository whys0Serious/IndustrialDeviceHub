using DeviceHub.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 设备分类业务服务接口
    /// </summary>
    public interface IDeviceCategoryService
    {
        /// <summary>
        /// 获取所有设备分类
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<List<DeviceCategoryDto>> GetAllAsync(CancellationToken ct = default);
        /// <summary>
        /// 按ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DeviceCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> CreateAsync(CreateDeviceCategoryDto request, CancellationToken ct = default);
        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task UpdateAsync(int id, UpdateDeviceCategoryDto request, CancellationToken ct = default);
        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DeleteAsync(int id, CancellationToken ct = default);
        /// <summary>
        /// 分类名称是否已存在
        /// </summary>
        /// <param name="name"></param>
        /// <param name="excludeId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    }
}
