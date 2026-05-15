using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryService
    {
       Task<BaseResp> Get(int uid);

        Task<BaseResp> GetById(int uid, int id);

        Task<BaseResp> GetByIdWithSubCategories(int uid, int? id = null);

        Task<BaseResp> Create(ReqCategory reqCategory, int uid);

        Task<BaseResp> UpdateCategory(ReqCategory reqCategory, int uid, int id);

        Task<BaseResp> DeleteCategory(int uid, int id);
    }
}
