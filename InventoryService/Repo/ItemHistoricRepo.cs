using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class ItemHistoricRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : IItemHistoricRepo
    {
        public async Task<int> AddAsync(ItemHistoric itemHistoric)
        {
            using var context = dbCtx.CreateDbContext();
            await context.ItemHistoric.AddAsync(itemHistoric);
            return await context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(List<ItemHistoric> itemHistorics)
        {
            using var context = dbCtx.CreateDbContext();
            await context.ItemHistoric.AddRangeAsync(itemHistorics);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddRangeItemListAsync(List<ItemHistoricItem> itemHistoricItems)
        {
            using var context = dbCtx.CreateDbContext();
            await context.ItemHistoricItem.AddRangeAsync(itemHistoricItems);
            return await context.SaveChangesAsync();
        }
    }
}
