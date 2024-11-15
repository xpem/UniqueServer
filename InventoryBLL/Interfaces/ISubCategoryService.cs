using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ISubCategoryService
    {
        BaseResponse GetById(int uid, int id);

        BaseResponse GetByCategoryId(int uid, int categoryId);

        Task<BaseResponse> CreateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid);

        Task<BaseResponse> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid, int id);

        Task<BaseResponse> InactiveSubCategoryAsync(int uid, int subCategoryId);

        Task<BaseResponse> GetByVersionAsync(int uid, int page, int version);
    }
}
