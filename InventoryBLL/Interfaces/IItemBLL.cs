using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface IItemBLL
    {
        BLLResponse CreateItem(ReqItem reqItem, int uid);

        BLLResponse UpdateItem(ReqItem reqItem, int uid);

        BLLResponse DeleteItem(int uid, int id);

        BLLResponse GetById(int uid, int id);

        BLLResponse Get(int uid);
    }
}