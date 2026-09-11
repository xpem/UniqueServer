using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class SubCategoryHistoricRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : ISubCategoryHistoricRepo
    {
        public async Task<int> AddAsync(SubCategoryHistoric subCategoryHistoric)
        {
            using var context = dbCtx.CreateDbContext();
            await context.SubCategoryHistoric.AddAsync(subCategoryHistoric);
            return await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<SubCategoryHistoric> subCategoryHistorics)
        {
            using var context = dbCtx.CreateDbContext();
            await context.SubCategoryHistoric.AddRangeAsync(subCategoryHistorics);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddRangeItemListAsync(List<SubCategoryHistoricItem> subCategoryHistoricItems)
        {
            using var context = dbCtx.CreateDbContext();
            await context.SubCategoryHistoricItem.AddRangeAsync(subCategoryHistoricItems);
            return await context.SaveChangesAsync();
        }

        public async Task<List<SubCategoryHistoric>> GetBySubCategoryIdAsync(int subCategoryId, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.SubCategoryHistoric
                .Where(x => x.SubCategoryId == subCategoryId && x.UserId == uid)
                .Include(x => x.SubCategoryHistoricType)
                .Include(x => x.SubCategoryHistoricItems)
                    .ThenInclude(i => i.SubCategoryHistoricItemField)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
