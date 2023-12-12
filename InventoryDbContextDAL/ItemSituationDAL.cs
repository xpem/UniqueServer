using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;

namespace InventoryDAL
{
    public class ItemSituationDAL(InventoryDbContext dbContext) : IItemSituationDAL
    {
        public List<ItemSituation>? Get(int uid) => dbContext.ItemSituation.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToList();

    }
}
