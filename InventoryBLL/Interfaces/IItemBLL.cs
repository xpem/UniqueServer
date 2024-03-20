using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface IItemBLL
    {
        BLLResponse CreateItem(ReqItem reqItem, int uid);

        BLLResponse UpdateItem(ReqItem reqItem, int uid, int id);

        BLLResponse DeleteItem(int uid, int id, string filePath);

        BLLResponse GetById(int uid, int id);

        Task<BLLResponse> GetAsync(int uid, int page);

        BLLResponse UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<BLLResponse> GetTotalItemsPagesAsync(int uid);
    }
}