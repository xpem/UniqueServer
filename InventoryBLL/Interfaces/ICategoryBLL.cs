using BaseModels;
using InventoryModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ICategoryBLL
    {
        BLLResponse Get(int uid);

        BLLResponse GetById(int uid, int id);

        BLLResponse GetWithSubCategories();

        BLLResponse CreateCategory(ReqCategory reqCategory, int uid);

        BLLResponse UpdateCategory(ReqCategory reqCategory, int uid, int id);

        BLLResponse DeleteCategory(int uid, int id);
    }
}
