using InventoryRepos.Interfaces;
using InventoryModels.DTOs;

namespace InventoryRepos
{
    public class ItemSituationDAL(InventoryDbContext dbContext) : IItemSituationDAL
    {
        public List<ItemSituation>? Get(int uid) => dbContext.ItemSituation.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToList();
        public ItemSituation? GetById(int uid, int id) => dbContext.ItemSituation.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefault();

    }
}
