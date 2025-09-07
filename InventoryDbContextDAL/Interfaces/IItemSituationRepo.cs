using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface IItemSituationRepo
    {
        Task<List<ItemSituation>?> Get(int uid);

        Task<ItemSituation?> GetById(int uid, int id);
    }
}