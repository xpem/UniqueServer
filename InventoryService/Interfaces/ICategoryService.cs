using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryService
    {
       Task<BaseResponse> Get(int uid);

        Task<BaseResponse> GetById(int uid, int id);

        Task<BaseResponse> GetByIdWithSubCategories(int uid, int? id = null);

        Task<BaseResponse> Create(ReqCategory reqCategory, int uid);

        Task<BaseResponse> UpdateCategory(ReqCategory reqCategory, int uid, int id);

        Task<BaseResponse> DeleteCategory(int uid, int id);
    }
}
