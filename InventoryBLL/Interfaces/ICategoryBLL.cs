using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryBLL
    {
        BLLResponse Get(int uid);

        BLLResponse GetById(int uid, int id);

        BLLResponse GetWithSubCategories(int uid);

        BLLResponse CreateCategory(ReqCategory reqCategory, int uid);

        BLLResponse UpdateCategory(ReqCategory reqCategory, int uid, int id);

        BLLResponse DeleteCategory(int uid, int id);
    }
}
