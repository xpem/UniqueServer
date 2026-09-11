using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class CategoryHistoricRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : ICategoryHistoricRepo
    {
        public async Task<int> AddAsync(CategoryHistoric categoryHistoric)
        {
            using var context = dbCtx.CreateDbContext();
            await context.CategoryHistoric.AddAsync(categoryHistoric);
            return await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<CategoryHistoric> categoryHistorics)
        {
            using var context = dbCtx.CreateDbContext();
            await context.CategoryHistoric.AddRangeAsync(categoryHistorics);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddRangeItemListAsync(List<CategoryHistoricItem> categoryHistoricItems)
        {
            using var context = dbCtx.CreateDbContext();
            await context.CategoryHistoricItem.AddRangeAsync(categoryHistoricItems);
            return await context.SaveChangesAsync();
        }

        public async Task<List<CategoryHistoric>> GetByCategoryIdAsync(int categoryId, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.CategoryHistoric
                .Where(x => x.CategoryId == categoryId && x.UserId == uid)
                .Include(x => x.CategoryHistoricType)
                .Include(x => x.CategoryHistoricItems)
                    .ThenInclude(i => i.CategoryHistoricItemField)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
