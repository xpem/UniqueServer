using BaseModels;
using InventoryModels.Req;

namespace InventoryBLL.Interfaces
{
    public interface ISubCategoryService
    {
        Task<BaseResp> GetByCategoryIdAsync(int uid, int categoryId);

        Task<BaseResp> GetByIdAsync(int uid, int id);

        Task<BaseResp> CreateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid);

        Task<BaseResp> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid, int id);

        Task<BaseResp> InactiveSubCategoryAsync(int uid, int subCategoryId);

        Task<BaseResp> GetByAfterUpdatedAtAsync(int uid, int page, DateTime updatedAt);

        //Task<BaseResponse> GetByIdWithCategory(int uid, int id);
    }
}
