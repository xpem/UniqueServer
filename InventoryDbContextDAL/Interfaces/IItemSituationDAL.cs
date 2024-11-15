using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface IItemSituationDAL
    {
        List<ItemSituation>? Get(int uid);

        ItemSituation? GetById(int uid, int id);
    }
}