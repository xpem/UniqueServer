using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ISubCategoryBLL
    {
        BLLResponse GetById(int uid, int id);

        BLLResponse GetByCategoryId(int uid, int categoryId);

        BLLResponse CreateSubCategory(ReqSubCategory reqSubCategory, int uid);

        BLLResponse UpdateSubCategory(ReqSubCategory reqSubCategory, int uid, int id);

        BLLResponse DeleteSubCategory(int uid, int subCategoryId);
    }
}
