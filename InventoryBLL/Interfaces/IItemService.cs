using BaseModels;
using InventoryModels.Req;
using InventoryModels.Res.Item;

namespace InventoryBLL.Interfaces
{
    public interface IItemService
    {
        BaseResponse CreateItem(ReqItem reqItem, int uid);

        BaseResponse UpdateItem(ReqItem reqItem, int uid, int id);

        BaseResponse DeleteItem(int uid, int id, string filePath);

        BaseResponse GetById(int uid, int id);

        Task<BaseResponse> GetAsync(int uid, int page);

        BaseResponse UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName);

        BaseResponse DeleteItemImage(int uid, int id, string fileName, string filePath);

        Task<BaseResponse> GetTotalItemsPagesAsync(int uid, int[]? situationIds);
        Task<BaseResponse> GetConfigs(int uid);

        Task<BaseResponse> GetBySearch(int uid, int page, ReqSearchItem reqSearchItem);
    }
}