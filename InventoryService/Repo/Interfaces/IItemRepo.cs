using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res.Item;

namespace InventoryRepos.Interfaces
{
    public interface IItemRepo
    {
        int Create(Item item);

        int CreateBulk(List<Item> items);

        int Inactivate(int uid, int id);

        int Update(Item item);

        Task<Item?> GetById(int uid, int id);

        int UpdateFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName);

        Task<int> GetTotalAsync(int uid);

        Task<List<Item>?> GetAsync(int uid, int page, int pageSize);

        Task<List<Item>?> GetBySearchAsync(int uid, int page, int pageSize, ReqSearchItem reqSearchItem);

        Task<int> GetTotalBySearchAsync(int uid, ReqSearchItem reqSearchItem);
        Task<List<string>> GetLastPurchaseStores(int uid, int count);

        Task<List<ResItemSituationsGroupingWithQuantities>> GetItemSituationsWithQuantities(int uid);
    }
}