using BaseModels;

namespace InventoryBLL.Interfaces
{
    public interface IAcquisitionTypeService
    {
        Task<BaseResp> Get(int uid);
    }
}