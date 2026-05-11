using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ISubCategoryService
    {
        Task<BaseResponse> GetByCategoryIdAsync(int uid, int categoryId);

        Task<BaseResponse> GetByIdAsync(int uid, int id);

        Task<BaseResponse> CreateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid);

        Task<BaseResponse> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid, int id);

        Task<BaseResponse> InactiveSubCategoryAsync(int uid, int subCategoryId);

        Task<BaseResponse> GetByAfterUpdatedAtAsync(int uid, int page, DateTime updatedAt);

        //Task<BaseResponse> GetByIdWithCategory(int uid, int id);
    }
}
