using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class ItemSituationRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : IItemSituationRepo
    {
        public async Task<List<ItemSituation>?> Get(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.ItemSituation.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToListAsync();
        }
        public async Task<ItemSituation?> GetById(int uid, int id)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.ItemSituation.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
        }
    }
}
