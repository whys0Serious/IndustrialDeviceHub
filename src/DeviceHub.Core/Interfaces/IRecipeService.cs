using DeviceHub.Core.Common;
using DeviceHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceHub.Core.Interfaces
{
    /// <summary>
    /// 参数配方接口
    /// </summary>
    public interface IRecipeService
    {

        /// <summary>
        /// 获取所有配方
        /// </summary>
        /// <returns></returns>
        Task<List<Recipe>> GetAllAsync();
        /// <summary>
        /// 分页获取所有配方
        /// </summary>
        Task<PagedResult<Recipe>> GetPagedAsync(string? keyword, int pageIndex, int pageSize);

        /// <summary>
        /// 新增配方
        /// </summary>
        Task AddAsync(Recipe recipe);

        /// <summary>
        /// 更新配方
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        Task UpdateAsync(Recipe recipe);

        /// <summary>
        /// 删除配方
        /// </summary>
        Task DeleteAsync(int id);
    }
}
