using BaseModels;

namespace InventoryBLL.Interfaces
{
    public interface IItemSituationService
    {
        Task<BaseResp> Get(int uid);
    }
}