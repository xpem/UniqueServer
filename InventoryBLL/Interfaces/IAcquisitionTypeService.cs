using BaseModels;

namespace InventoryBLL.Interfaces
{
    public interface IAcquisitionTypeService
    {
        Task<BaseResponse> Get(int uid);
    }
}