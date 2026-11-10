using DeviceHub.Core.Common;
using DeviceHub.Core.Entities;
using DeviceHub.Core.Interfaces;
using DeviceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceHub.Infrastructure.Services
{
    /// <summary>
    /// 配方业务服务实现
    /// </summary>
    public class RecipeService : IRecipeService
    {
        private readonly DeviceHubDbContext _db;

        public RecipeService(DeviceHubDbContext db) => _db = db;

        public async Task<PagedResult<Recipe>> GetPagedAsync(string? keyword, int pageIndex, int pageSize)
        {
            var query = _db.Recipes.AsQueryable();

            // 支持按配方名称模糊搜索
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r => r.Name.Contains(keyword.Trim()));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Recipe>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageIndex,
                PageSize = pageSize
            };
        }


        public async Task AddAsync(Recipe recipe)
        {
            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Recipe recipe)
        {
            var existing = await _db.Recipes.FindAsync(recipe.Id);
            if (existing == null) return;

            //只更新允许修改的字段
            existing.Name = recipe.Name;
            existing.TargetTemperature = recipe.TargetTemperature;
            existing.TargetPressure = recipe.TargetPressure;
            existing.TargetSpeed = recipe.TargetSpeed;


            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _db.Recipes.Remove(recipe);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Recipe>> GetAllAsync()
        {
            return await _db.Recipes
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync();
        }
    }
}
