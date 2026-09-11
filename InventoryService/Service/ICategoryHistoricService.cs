using BaseModels;
using InventoryModels.DTOs;

namespace InventoryServices.Service
{
    public interface ICategoryHistoricService
    {
        Task<int> AddAsync(CategoryHistoric categoryHistoric);
        Task BuildAndCreateCategoryUpdateHistoricAsync(Category oldCategory, Category newCategory, int userId);
        Task<BaseResp> GetByCategoryIdAsync(int categoryId, int uid);
    }
}
