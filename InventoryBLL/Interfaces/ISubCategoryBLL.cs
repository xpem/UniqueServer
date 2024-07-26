using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ISubCategoryBLL
    {
        BaseResponse GetById(int uid, int id);

        BaseResponse GetByCategoryId(int uid, int categoryId);

        BaseResponse CreateSubCategory(ReqSubCategory reqSubCategory, int uid);

        BaseResponse UpdateSubCategory(ReqSubCategory reqSubCategory, int uid, int id);

        BaseResponse DeleteSubCategory(int uid, int subCategoryId);
    }
}
