using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class ItemSituationRepo(InventoryDbContext dbContext) : IItemSituationRepo
    {
        public async Task<List<ItemSituation>?> Get(int uid) => await dbContext.ItemSituation.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToListAsync();
        public async Task<ItemSituation?> GetById(int uid, int id) => await dbContext.ItemSituation.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefaultAsync();

    }
}
