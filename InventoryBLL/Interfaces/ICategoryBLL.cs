using BaseModels;
using InventoryModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryBLL
    {
        Task<BLLResponse> Get(int uid);

        Task<BLLResponse> GetById(int uid, int id);

        Task<BLLResponse> GetWithSubCategories();

        Task<BLLResponse> CreateCategory(ReqCategory reqCategory);
    }
}
