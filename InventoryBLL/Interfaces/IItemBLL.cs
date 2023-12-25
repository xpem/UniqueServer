using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface IItemBLL
    {
        BLLResponse CreateItem(ReqItem reqItem, int uid);

        BLLResponse UpdateItem(ReqItem reqItem, int uid, int id);

        BLLResponse DeleteItem(int uid, int id,string filePath);

        BLLResponse GetById(int uid, int id);

        BLLResponse Get(int uid);

        BLLResponse UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2);
    }
}