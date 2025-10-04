using BaseModels;

namespace InventoryBLL.Interfaces
{
    public interface IItemSituationService
    {
        Task<BaseResponse> Get(int uid);
    }
}