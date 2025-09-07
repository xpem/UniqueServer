using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryService
    {
        BaseResponse Get(int uid);

        BaseResponse GetById(int uid, int id);

        Task<BaseResponse> GetByIdWithSubCategories(int uid, int? id = null);

        BaseResponse Create(ReqCategory reqCategory, int uid);

        BaseResponse UpdateCategory(ReqCategory reqCategory, int uid, int id);

        BaseResponse DeleteCategory(int uid, int id);
    }
}
