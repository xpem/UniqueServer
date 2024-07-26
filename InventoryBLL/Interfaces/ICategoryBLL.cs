using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryBLL
    {
        BaseResponse Get(int uid);

        BaseResponse GetById(int uid, int id);

        BaseResponse GetWithSubCategories(int uid);

        BaseResponse Create(ReqCategory reqCategory, int uid);

        BaseResponse UpdateCategory(ReqCategory reqCategory, int uid, int id);

        BaseResponse DeleteCategory(int uid, int id);
    }
}
