using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface IItemSituationDAL
    {
        List<ItemSituation>? Get(int uid);

        ItemSituation? GetById(int uid, int id);
    }
}