using BaseModels;
using InventoryModels.DTOs;

namespace InventoryServices.Service
{
    public interface ISubCategoryHistoricService
    {
        Task<int> AddAsync(SubCategoryHistoric subCategoryHistoric);
        Task BuildAndCreateSubCategoryUpdateHistoricAsync(SubCategory oldSubCategory, SubCategory newSubCategory, int userId);
        Task<BaseResp> GetBySubCategoryIdAsync(int subCategoryId, int uid);
    }
}
