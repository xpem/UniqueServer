using BaseModels;
using InventoryModels.Req;
using InventoryModels.Res.Item;

namespace InventoryServices.Interfaces
{
    public interface IItemService
    {
        Task<BaseResp> CreateItem(ReqItem reqItem, int uid);

        Task<BaseResp> UpdateItem(ReqItem reqItem, int uid, int id);

        Task<BaseResp> DeleteItem(int uid, int id, string filePath);

        Task<BaseResp> GetById(int uid, int id);

        Task<BaseResp> GetAsync(int uid, int page);

        BaseResp UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName);

        Task<BaseResp> GetTotalItemsPagesAsync(int uid);
        Task<BaseResp> GetConfigs(int uid);

        Task<BaseResp> GetBySearch(int uid, int page, ReqSearchItem reqSearchItem);

        Task<BaseResp> GetTotalItemsPagesBySearchAsync(int uid, ReqSearchItem reqSearchItem);

        Task<BaseResp> DeleteItemImage(int uid, int id, string fileName, string filePath);

        Task<BaseResp> GetItemsSituationsGroupingWithQuantities(int uid);
    }
}