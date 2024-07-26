using BaseModels;

namespace InventoryBLL.Interfaces
{
    public interface IItemSituationBLL
    {
        BaseResponse Get(int uid);
    }
}